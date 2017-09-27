using System.Collections.Generic;
using System.Linq;
using JustConveyor.Contracts;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Contexts;

namespace JustConveyor
{
    /// <summary>
    /// Usefull extensions for building pipeline.
    /// </summary>
	public static class PipelineCoveyorExtension
	{
        /// <summary>
        /// Add calling another pipeline.
        /// </summary>
        /// <param name="blueprint"></param>
        /// <param name="pipelineName"></param>
        /// <returns></returns>
		public static PipelineBlueprint CallPipeline(this PipelineBlueprint blueprint, string pipelineName)
		{
			var convey = Injection.InjectionProvider.Get<ConveyorContract>();
			blueprint.Apply(async (octx, tctx) =>
			{
				var package = await convey.Process(new Package
				{
					Label = pipelineName,
					Load = octx.Unit,
					Id = octx.UnitId
				});

				return package.Load;
			});
			return blueprint;
		}

        /// <summary>
        /// Add elements casting step.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="blueprint"></param>
        /// <returns></returns>
	    public static PipelineBlueprint CastCollection<TType>(this PipelineBlueprint blueprint)
	    {
	        return
	            blueprint.Apply(
	                (List<UnitContext> unit) => unit.Where(el => el.Unit != null).Select(el => el.GetUnit<TType>()));
	    }
        
        /// <summary>
        /// Collect elements after splitting and casting to given type.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="blueprint"></param>
        /// <returns></returns>
	    public static PipelineBlueprint CollectAndCast<TType>(this PipelineBlueprint blueprint)
	    {
	        return
	            blueprint
                    .Collect()
                    .Apply(
	                (List<UnitContext> unit) => unit.Where(el => el.Unit != null).Select(el => el.GetUnit<TType>()));
	    }
	}
}