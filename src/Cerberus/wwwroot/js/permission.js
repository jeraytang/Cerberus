$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 10,
            count: 0,
            permissionTypes: [
                {id: 'Menu', text: 'Menu'},
                {id: 'Function', text: 'Function'},
                {id: 'Api', text: 'Api'},
                {id: 'Other', text: 'Other'}
            ],
            service: '',
            keyword: '',
            items: [],
            newItem: {
                serviceId: '',
                name: '',
                type: 'Function',
                description: '',
                module: '',
                identification: ''
            },
            originItem: {},
            services: [],
            searchServices: []
        },
        mounted: function () {
            let that = this;
            $('#addModal').on('show.bs.modal', function () {
                that.newItem = {
                    serviceId: that.services.length > 0 ? that.services[0].id : '',
                    type: 'Function',
                    name: '',
                    description: '',
                    module: '',
                    identification: ''
                };
            });
            $('#viewModal').on('show.bs.modal', function (event) {
                let id = $(event.relatedTarget).parent().parent().attr('id');
                http.get(`/api/v1.0/permissions/${id}`, function (result) {
                    that.originItem = result.data;
                });
            });
            that.searchServices = [{
                id: '',
                text: 'All'
            }];
            that.service = [];
            http.get(`/api/v1.0/services?limit=65535`, function (result) {
                result.data.data.forEach(x => {
                    that.services.push({
                        id: x.id,
                        text: x.name
                    });
                    that.searchServices.push({
                        id: x.id,
                        text: x.name
                    });
                });
            });
            this.load();
        },
        methods: {
            search: function () {
                let that = this;
                let pagedQuery = `page=1&limit=12&keyword=${that.keyword}&serviceId=${that.service}`;
                http.get(`/api/v1.0/permissions?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            getPagedQuery: function () {
                let page = getQueryArgument('page');
                page = (page == null ? 1 : page);

                let limit = getQueryArgument('limit');
                limit = (limit == null ? 12 : limit);

                let keyword = getQueryArgument('keyword');
                keyword = (keyword == null ? '' : keyword);

                let service = getQueryArgument('serviceId');
                service = (service == null ? '' : service);

                let module1 = getQueryArgument('module');
                module1 = (module1 == null ? '' : module1);
                return `page=${page}&limit=${limit}&keyword=${keyword}&serviceId=${service}&module=${module1}`;
            },
            load: function () {
                let that = this;
                let pagedQuery = this.getPagedQuery();
                http.get(`/api/v1.0/permissions?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            submit: function () {
                let that = this;
                http.post(`/api/v1.0/permissions`, this.newItem, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add permission failed', "error");
                    }
                });
            },
            remove: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this permission?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/permissions/${id}`, function (result) {
                        if (result.data) {
                            that.load();
                        } else {
                            swal('Error', 'Remove permission failed', "error");
                        }
                    });
                });
            },
            update: function () {
                let that = this;
                let id = that.originItem.id;
                http.put(`/api/v1.0/permissions/${id}`, this.originItem, function (result) {
                    if (result.data) {
                        $('#viewModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Update permission failed', "error");
                    }
                });
            }
        }
    });
});