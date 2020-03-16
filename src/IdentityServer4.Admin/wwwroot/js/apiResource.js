$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 15,
            count: 0,
            trueOrFalse: [
                {id: 'True', text: 'True'},
                {id: 'False', text: 'False'}
            ],
            resources: [],
            newResource: {
                name: '',
                displayName: '',
                description: '',
                userClaims: ''
            },
            originResource: {}
        },
        mounted: function () {
            let that = this;
            $('#addModal').on('show.bs.modal', function () {
                that.newResource = {
                    name: '',
                    displayName: '',
                    description: '',
                    userClaims: ''
                };
            });
            $('#viewModal').on('show.bs.modal', function (event) {
                let id = $(event.relatedTarget).parent().parent().attr('id');
                http.get(`/api/v1.0/apiResources/${id}`, function (result) {
                    that.originResource = result.data;
                });
            });
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let pagedQuery = getPagedQuery();
                http.get(`/api/v1.0/apiResources?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.resources = [];
                    result.data.entities.forEach(x => {
                        that.resources.push(x);
                    });
                });
            },
            submit: function () {
                let that = this;
                http.post(`/api/v1.0/apiResources`, this.newResource, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add resource failed', "error");
                    }
                });
            },
            disable: function (event) {
                let id = $(event.currentTarget).parent().parent().attr('id');
                let that = this;
                swal({
                    title: "Sure to disable this resource?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.put(`/api/v1.0/apiResources/${id}/disable`, {}, function () {
                        that.load();
                    });
                });
            },
            remove: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this resource?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/apiResources/${id}`, function (result) {
                        if (result.data) {
                            that.load();
                        } else {
                            swal('Error', 'Remove resource failed', "error");
                        }
                    });
                });
            },
            enable: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                http.put(`/api/v1.0/apiResources/${id}/enable`, {}, function () {
                    that.load();
                });
            },
            update: function () {
                let that = this;
                let id = that.originResource.id;
                http.put(`/api/v1.0/apiResources/${id}`, this.originResource, function (result) {
                    if (result.data) {
                        $('#viewModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Update resource failed', "error");
                    }
                });
            }
        }
    });
});