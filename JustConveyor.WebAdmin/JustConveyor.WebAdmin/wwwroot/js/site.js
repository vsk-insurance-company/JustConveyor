'use strict';

if (!Date.prototype.toLocalISOString) {
    (function() {

        function pad(number) {
            if (number < 10) {
                return '0' + number;
            }
            return number;
        }

        Date.prototype.toLocalISOString = function() {
            return this.getFullYear() +
                '-' + pad(this.getMonth() + 1) +
                '-' + pad(this.getDate()) +
                'T' + pad(this.getHours()) +
                ':' + pad(this.getMinutes()) +
                ':' + pad(this.getSeconds()) +
                '.' + (this.getMilliseconds() / 1000).toFixed(3).slice(2, 5) +
                '+' + pad(-this.getTimezoneOffset()/60) + ':00';
        };

    }());
}

const maintext = new Vue({
    el: '#main-text',
    components: {
        VueBootstrapTable: VueBootstrapTable
    },
    data: {
        started: 'unknown',
        inflight: 'unknown',
        lastUpdate: null,
        
        visibleInfo: 'blueprints',

        blueprintsTable: {
            columns: [
                {title: "Name"},
                {title: "Builder class", name: "BuilderClass", visible: false},
                {title: "Routing name", name: "RoutingName", visible: false},
                {title: "Concurrency level", name: "ConcurrencyLevel"},
                {title: "Incoming count", name: "In"},
                {title: "Outcoming count", name: "Out"},
                {title: "Processing rate (1/sec)", name: "ProcessingRatePerSec"}
            ],
            values: [],
            logging: [],
            showFilter: true,
            showPicker: true,
            paginated: true,
            multiColumnSortable: true
        },

        pipelinesTable: {
            columns: [
                {title: "Id"},
                {title: "State"}
            ],
            values: [],
            logging: [],
            showFilter: true,
            showPicker: true,
            paginated: true,
            multiColumnSortable: true
        },

        queuesTable: {
            columns: [
                {title: "Processor blueprint", name: 'Blueprint'},
                {title: "Queue type", name: "QueueType"},
                {title: "On queue (delta)", name: "OnQueueDelta"},
                {title: "On queue (previous)", name: "OnQueuePrevious"},
                {title: "On queue (now)", name: "OnQueue"}],
            values: [],
            logging: [],
            showFilter: true,
            showPicker: true,
            paginated: true,
            multiColumnSortable: true
        },

        contextsTable: {
            columns: [
                {title: "Id"},
                {title: "Process time", name: "InProcessing"},
                {title: "Process started", name: "ProcessingStart"},
                {title: "Current step", name: "Step"}],
            values: [],
            logging: [],
            showFilter: true,
            showPicker: true,
            paginated: true,
            multiColumnSortable: true
        },

        suppliersTable: {
            columns: [
                {title: "Name"},
                {title: "Supplier type", name: "SupplierType"},
                {title: "Current state", name: "State"},
                {title: "Rate (1/sec)", name: "PackagesRatePerSec"},
                {title: "Supplied packages count", name: "Supplied"}],
            values: [],
            logging: [],
            showFilter: true,
            showPicker: true,
            paginated: true,
            multiColumnSortable: true
        }
    },
    methods: {
        showBlueprintInfo: function () {
            this.visibleInfo = 'blueprints';
        },
        showPipelinesInfo: function () {
            this.visibleInfo = 'pipelines';
        },
        showQueuesInfo: function () {
            this.visibleInfo = 'queues';
        },
        showContextsInfo: function () {
            this.visibleInfo = 'contexts';
        },
        showSuppliersInfo: function () {
            this.visibleInfo = 'suppliers';
        },
    }
});

const serverSelector = new Vue({
    el: '#server-selector',
    data: {
        server: 'http://localhost:9910/justconveyor/api/metrics',
        autoupdateFlag: false,
        autoupdateIntervalTimer: null
    },
    methods: {
        loadData: function () {
            $.get(this.server).then((result) => {
                maintext.started = result.Started;
                maintext.inflight = result.InFlightTime;
                maintext.lastUpdate = new Date().toLocalISOString();

                maintext.blueprintsTable.values = result.Blueprints;
                maintext.queuesTable.values = result.Queues;
                maintext.contextsTable.values = result.Contextes;
                maintext.pipelinesTable.values = result.Pipelines;
                maintext.suppliersTable.values = result.Suppliers;
            }, (err) => {
                this.autoupdateFlag = false;
                this.autoupdateChanged();
                console.error(err);
            })
        },
        autoupdateChanged: function () {
            if (this.autoupdateFlag === true)
                this.autoupdateIntervalTimer = setInterval(this.loadData, 5000);
            else
                clearInterval(this.autoupdateIntervalTimer)
        }
    }
});

