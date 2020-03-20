$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 10,
            count: 0,
            resource: {},
            secrets: [],
            secret: {
                value: '',
                type: 'SharedSecret',
                description: '',
                expiration: null
            }
        },
        mounted: function () {
            this.load();
        }, methods: {
            load: function () {
                let that = this;
                let resourceId = this.getResourceId();
                http.get(`/api/v1.0/apiresources/${resourceId}`, function (result) {
                    that.resource = result.data;
                });
                http.get(`/api/v1.0/apiresources/${resourceId}/secrets`, function (result) {
                    that.secrets = result.data;
                });
            },
            getResourceId: function () {
                let data = window.location.href.split('/');
                return data[data.length - 2];
            },
            add: function () {
                let that = this;
                let resourceId = this.getResourceId();
                http.post(`/api/v1.0/apiresources/${resourceId}/secrets`, that.secret, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add secret failed', "error");
                    }
                    that.secret = {
                        value: '',
                        type: 'SharedSecret',
                        description: '',
                        expiration: null
                    }
                });
            },
            remove: function () {
                let that = this;
                let resourceId = this.getResourceId();
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this secret?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/apiresources/${resourceId}/secrets/${id}`, function (result) {
                        if (result.data) {
                            that.load();
                        } else {
                            swal('Error', 'Add secret failed', "error");
                        }
                    });
                });
            }
        }
    });
});