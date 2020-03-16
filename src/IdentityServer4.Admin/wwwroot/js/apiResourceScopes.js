$(function () {
    new Vue({
        el: '#app',
        data: {
            resource: {},
            trueOrFalse: [
                {id: 'True', text: 'True'},
                {id: 'False', text: 'False'}
            ],
            scopes: [],
            newScope: {
                name: '',
                displayName: '',
                description: '',
                userClaims: '',
                required: 'True',
                showInDiscoveryDocument: 'True',
                emphasize: 'True'
            },
            originScope: {}
        },
        mounted: function () {
            let that = this;
            $('#addModal').on('show.bs.modal', function () {
                that.newScope = {
                    name: '',
                    displayName: '',
                    description: '',
                    userClaims: '',
                    required: 'True',
                    showInDiscoveryDocument: 'True',
                    emphasize: 'True'
                };
            });
            $('#viewModal').on('show.bs.modal', function (event) {
                let resourceId = that.getResourceId();
                let id = $(event.relatedTarget).parent().parent().attr('id');
                http.get(`/api/v1.0/apiResources/${resourceId}/scopes/${id}`, function (result) {
                    that.originScope = result.data;
                });
            });
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let resourceId = this.getResourceId();
                http.get(`/api/v1.0/apiresources/${resourceId}`, function (result) {
                    that.resource = result.data;
                });
                http.get(`/api/v1.0/apiresources/${resourceId}/scopes`, function (result) {
                    that.scopes = [];
                    that.scopes = result.data;
                });
            },
            submit: function () {
                let that = this;
                let resourceId = this.getResourceId();
                http.post(`/api/v1.0/apiResources/${resourceId}/scopes`, this.newScope, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add api resource scope failed', "error");
                    }
                });
            },
            remove: function (event) {
                let that = this;
                let resourceId = this.getResourceId();
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this scope?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/apiResources/${resourceId}/scopes/${id}`, function (result) {
                        that.load();
                    });
                });
            },
            getResourceId: function () {
                let data = window.location.href.split('/');
                return data[data.length - 2];
            },
            update: function () {
                let that = this;
                let resourceId = this.getResourceId();
                let id = that.originScope.id;
                http.put(`/api/v1.0/apiResources/${resourceId}/scopes/${id}`, this.originScope, function (result) {
                    if (result.data) {
                        $('#viewModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Update scope failed', "error");
                    }
                });
            }
        }
    });
});