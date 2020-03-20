$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 10,
            count: 0,
            clients: [],
            grantTypes: [
                {id: 'code', text: 'Code'},
                {id: 'implicit', text: 'Implicit'},
                {id: 'hybrid', text: 'Hybrid'},
                {id: 'client_credentials', text: 'ClientCredentials'},
                {id: 'urn:ietf:params:oauth:grant-type:device_code', text: 'DeviceFlow'},
                {id: 'password', text: 'ResourceOwnerPassword'},
                {id: 'code client_credentials', text: 'CodeAndClientCredentials'},
                {id: 'hybrid client_credentials', text: 'HybridAndClientCredentials'},
                {id: 'implicit client_credentials', text: 'ImplicitAndClientCredentials'},
                {id: 'password client_credentials', text: 'ResourceOwnerPasswordAndClientCredentials'}
            ],
            client: {
                clientId: '',
                clientName: '',
                allowedGrantTypes: 'implicit',
                allowedScopes: 'openid profile',
                description: ''
            }
        },
        mounted: function () {
            let that = this;
            $('#addModal').on('show.bs.modal', function () {
                that.client = {
                    clientId: '',
                    clientName: '',
                    allowedGrantTypes: 'implicit',
                    allowedScopes: 'openid profile',
                    description: ''
                };
            });
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let pagedQuery = getPagedQuery();
                http.get(`/api/v1.0/clients?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.clients = [];
                    result.data.entities.forEach(x => {
                        that.clients.push(x);
                    });
                });
            },
            submit: function () {
                let that = this;
                http.post(`/api/v1.0/clients`, this.client, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add client failed', "error");
                    }
                });
            },
            disable: function (event) {
                let id = $(event.currentTarget).parent().parent().attr('id');
                let that = this;
                swal({
                    title: "Sure to disable this client?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.put(`/api/v1.0/clients/${id}/disable`, {}, function () {
                        that.load();
                    });
                });
            },
            enable: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                http.put(`/api/v1.0/clients/${id}/enable`, {}, function () {
                    that.load();
                });
            },
            remove: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this client?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/clients/${id}`, function (result) {
                        if (result.data) {
                            that.load();
                        } else {
                            swal('Error', 'Remove client failed', "error");
                        }
                    });
                });
            }
        }
    });
});