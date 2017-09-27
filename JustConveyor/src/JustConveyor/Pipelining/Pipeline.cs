using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Exceptions;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Pipelining.ProcessingElements;

namespace JustConveyor.Pipelining
{
    public sealed class Pipeline
    {
        private ProcessingElement First { get; }
        private ProcessingElement Last { get; }

        private readonly Func<TransferingContext, UnitContext, Task> mAction;
        public PipelineBlueprint Blueprint { get; }
        public string Id { get; }

        public Pipeline(PipelineBlueprint blueprint)
        {
            Blueprint = blueprint;

            First = new Initiator {Next = blueprint.First.Clone(null)};
            var tail = First.GetTail();
            Last = new Terminator {Previous = tail};
            tail.Next = Last;

            if (!ProcessPipeline(First, new Stack<Splitter>()))
                throw new InvalidSplitCollectException(blueprint.Name);

            BuildPipelineAction(Last);
            mAction = First.ProcessTask;

            Id = $"{blueprint.Name}:{Guid.NewGuid().ToString("n")}";
        }

        public async Task Process(TransferingContext tctx, object seed, string unitId = "",
            string operationId = "", Headers headers = null)
        {
            tctx.Set("pipelineid", Id);
            try
            {
                var octx =
                    new UnitContext(string.IsNullOrEmpty(operationId) ? Guid.NewGuid().ToString("n") : operationId,
                        unitId, seed, headers?.Dict);
                await mAction(tctx, octx);
            }
            catch (Exception e)
            {
                tctx.Exception = e;
            }
        }

        private static bool ProcessPipeline(ProcessingElement current, Stack<Splitter> stack)
        {
            if (current == null)
                return stack.Count == 0;

            if (current is Splitter)
                stack.Push((Splitter) current);
            else if (current is Collector)
            {
                if (stack.Count == 0)
                    return false;

                var splitter = stack.Pop();
                splitter.ConnectedCollector = (Collector) current;
            }
            else
            {
                if (current.IsAsync)
                    foreach (var splitter in stack)
                        splitter.IsAsync = true;
            }

            return ProcessPipeline(current.Next, stack);
        }

        private void BuildPipelineAction(ProcessingElement current)
        {
            if (current == null)
                return;

            var obj = current.Carrier != null ? Injection.InjectionProvider.Get(current.Carrier) : null;

            if (current is Processor)
                PrepareProcessor((Processor) current, obj);
            else if (current is Splitter)
                PrepareSplitter((Splitter) current, obj);
            else if (current is Collector)
                PrepareCollector((Collector) current, obj);

            BuildPipelineAction(current.Previous);
        }

        private static void PrepareCollector(Collector current, object obj)
        {
            current.CollectAction = (tctx, octx, dict) =>
            {
                try
                {
                    var parameters = PrepareParameters(current, octx, tctx);
                    string id = current.Method != null ? (string) current.Method.Invoke(obj, parameters.ToArray()) : "";

                    if (!dict.ContainsKey(id))
                        dict.Add(id, new List<UnitContext>());

                    dict[id].Add(octx);
                }
                catch (PipelineProcessingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    var resultException = ProcessException(current, tctx, e, octx, obj);
                    throw new PipelineProcessingException(resultException);
                }
            };
        }

        private static void PrepareProcessor(Processor current, object obj)
        {
            var nextAction = current.Next?.ProcessAction;
            if (current.IsAsync)
            {
                var nextAsyncProcessor = ProcessingElement.NextAsyncProcessor(current.Next);
                current.ProcessTask = async (tctx, octx) =>
                {
                    ProcessingInfo info = new ProcessingInfo
                    {
                        Started = DateTime.Now,
                        StepName = !string.IsNullOrEmpty(current.Name) ? current.Name : current.Method.Name
                    };

                    tctx.ProcessingHistory.Push(info);

                    try
                    {
                        var parameters = PrepareParameters(current, octx, tctx);

                        if (current.Method.ReturnType.IsGenericType &&
                            current.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                            octx.Unit =
                                await Wrap(current.Method.Invoke(obj ?? current.ConcreteObject, parameters.ToArray()));
                        
                        if (current.Method.ReturnType == typeof(Task))
                            await (Task) current.Method.Invoke(obj ?? current.ConcreteObject, parameters.ToArray());

                        info.Finished = DateTime.Now;

                        nextAction?.Invoke(tctx, octx);

                        if (nextAsyncProcessor != null)
                            await nextAsyncProcessor.ProcessTask(tctx, octx);
                    }
                    catch (PipelineProcessingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Exception resultException = ProcessException(current, tctx, e, octx, obj, true);
                        throw new PipelineProcessingException(resultException);
                    }
                };
            }
            else
            {
                current.ProcessAction = (tctx, octx) =>
                {
                    ProcessingInfo info = new ProcessingInfo
                    {
                        Started = DateTime.Now,
                        StepName = !string.IsNullOrEmpty(current.Name) ? current.Name : current.Method.Name
                    };

                    tctx.ProcessingHistory.Push(info);

                    try
                    {
                        var parameters = PrepareParameters(current, octx, tctx);

                        if (current.Method.ReturnType == typeof(void))
                            current.Method.Invoke(obj ?? current.ConcreteObject, parameters.ToArray());
                        else
                            octx.Unit = current.Method.Invoke(obj ?? current.ConcreteObject, parameters.ToArray());

                        info.Finished = DateTime.Now;

                        nextAction?.Invoke(tctx, octx);
                    }
                    catch (PipelineProcessingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        var resultException = ProcessException(current, tctx, e, octx, obj, true);
                        if (resultException != null)
                            throw new PipelineProcessingException(resultException);
                    }
                    finally
                    {
                        info.Finished = DateTime.Now;
                    }
                };
            }
        }

        private void PrepareSplitter(Splitter current, object obj)
        {
            var nextAction = current.Next?.ProcessAction;
            var afterSplitAction = current.ConnectedCollector?.Next?.ProcessAction;
            if (current.IsAsync)
            {
                var nextAsyncProcessor = ProcessingElement.NextAsyncProcessor(current.Next);
                var afterAsyncProcessor = ProcessingElement.NextAsyncProcessor(current.ConnectedCollector?.Next);
                current.ProcessTask = async (tctx, octx) =>
                {
                    try
                    {
                        var parameters = PrepareParameters(current, octx, tctx);
                        var splitterInvokeResult = current.Method.Invoke(obj, parameters.ToArray());
                        var result = typeof(IEnumerable<UnitContext>).IsAssignableFrom(splitterInvokeResult.GetType())
                            ? ((IEnumerable<UnitContext>) splitterInvokeResult).GetEnumerator()
                            : (IEnumerator<UnitContext>) splitterInvokeResult;

                        var dict = new Dictionary<string, IList<UnitContext>>();

                        while (result.MoveNext())
                        {
                            var soctx = result.Current;
                            nextAction?.Invoke(tctx, soctx);
                            if (tctx.Exception != null)
                                throw tctx.Exception;

                            if (nextAsyncProcessor != null)
                                await nextAsyncProcessor.ProcessTask(tctx, soctx);

                            current.ConnectedCollector.CollectAction(tctx, soctx, dict);
                        }

                        SetCollectedUnit(current, octx, dict);

                        afterSplitAction?.Invoke(tctx, octx);
                        if (afterAsyncProcessor != null)
                            await afterAsyncProcessor.ProcessTask(tctx, octx);
                    }
                    catch (PipelineProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        var resultException = ProcessException(current, tctx, exception, octx, obj);
                        throw new PipelineProcessingException(resultException);
                    }
                };
            }
            else
            {
                current.ProcessAction = (tctx, octx) =>
                {
                    try
                    {
                        var parameters = PrepareParameters(current, octx, tctx);
                        var result = (IEnumerator<UnitContext>) current.Method.Invoke(obj, parameters.ToArray());
                        var dict = new Dictionary<string, IList<UnitContext>>();

                        while (result.MoveNext())
                        {
                            var soctx = result.Current;
                            nextAction?.Invoke(tctx, soctx);
                            if (tctx.Exception != null)
                                throw tctx.Exception;

                            current.ConnectedCollector.CollectAction(tctx, soctx, dict);
                        }

                        SetCollectedUnit(current, octx, dict);
                        afterSplitAction?.Invoke(tctx, octx);
                    }
                    catch (PipelineProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        var resultException = ProcessException(current, tctx, exception, octx, obj);
                        throw new PipelineProcessingException(resultException);
                    }
                };
            }
        }

        private static void SetCollectedUnit(Splitter current, UnitContext octx,
            Dictionary<string, IList<UnitContext>> dict)
        {
            var runit = dict.Select(el =>
            {
                foreach (var ctx in el.Value)
                    ctx.Set("$collection_id", el.Key);
                return el.Value;
            }).ToList();

            octx.Unit = current.ConnectedCollector.Method != null
                ? (object) runit
                : runit.First();
        }

        private static List<object> PrepareParameters(ProcessingElement current, UnitContext octx,
            TransferingContext tctx)
        {
            List<object> parameters = new List<object>();
            if (current.Method != null)
                // TODO memoize me
                // TODO inject headers
                foreach (var parameterInfo in current.Method.GetParameters())
                {
                    if (parameterInfo.ParameterType.IsAssignableFrom(octx.GetUnitType()))
                        parameters.Add(octx.Unit);
                    else if (parameterInfo.ParameterType == typeof(TransferingContext))
                        parameters.Add(tctx);
                    else if (parameterInfo.ParameterType == typeof(UnitContext))
                        parameters.Add(octx);
                    else
                        throw new ParameterTypeMissmatchException(current.Carrier, current.Method.Name,
                            octx.GetUnitType(),
                            parameterInfo.ParameterType);
                }

            return parameters;
        }

        private static Exception ProcessException(ProcessingElement current, TransferingContext tctx,
            Exception exception,
            UnitContext octx, object obj, bool exceptionAllowed = false)
        {
            if (current?.ErrorProcessorMethod == null)
                return exception;

            octx.Exception = exception;
            var errParams = new List<object>();
            foreach (var param in current.ErrorProcessorMethod.GetParameters())
            {
                if (param.ParameterType == typeof(Exception))
                    errParams.Add(exception);
                if (param.ParameterType == typeof(UnitContext))
                    errParams.Add(octx);
                if (param.ParameterType == typeof(TransferingContext))
                    errParams.Add(tctx);
            }
            try
            {
                var result = (bool) current.ErrorProcessorMethod.Invoke(obj, errParams.ToArray()) && exceptionAllowed;
                return !result ? exception : null;
            }
            catch (Exception errorProcessingException)
            {
                var exceptions = new AggregateException(exception, errorProcessingException);
                tctx.Exception = exceptions;
                return exceptions;
            }
        }

        private static async Task<object> Wrap(dynamic task)
        {
            return await task;
        }
    }
}