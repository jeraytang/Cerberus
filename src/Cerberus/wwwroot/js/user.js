$(function () {
    new Vue({
        el: '#app',
        data: {
            page: 1,
            limit: 10,
            count: 0,
            sex: [
                {id: 'Unknown', text: 'Unknown'},
                {id: 'Male', text: 'Male'},
                {id: 'Female', text: 'Female'}
            ],
            items: [],
            newItem: {
                email: '',
                password: '',
                source: ''
            },
            originItem: {},
            keyword: ''
        },
        mounted: function () {
            let that = this;
            $('#addModal').on('show.bs.modal', function () {
                that.newItem = {
                    email: '',
                    password: '',
                    source: ''
                };
            });
            $('#viewModal').on('show.bs.modal', function (event) {
                let id = $(event.relatedTarget).parent().parent().attr('id');
                http.get(`/api/v1.0/users/${id}`, function (result) {
                    that.originItem = result.data;
                });
            });
            this.load();
        },
        methods: {
            search: function () {
                let that = this;
                let pagedQuery = `page=1&limit=12&keyword=${that.keyword}`;
                http.get(`/api/v1.0/users?${pagedQuery}`, function (result) {
                    that.page = result.data.page;
                    that.limit = result.data.limit;
                    that.count = result.data.count;
                    that.items = [];
                    result.data.data.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            load: function () {
                let that = this;
                let pagedQuery = getPagedQuery();
                http.get(`/api/v1.0/users?${pagedQuery}`, function (result) {
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
                http.post(`/api/v1.0/users`, this.newItem, function (result) {
                    if (result.data) {
                        $('#addModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Add user failed', "error");
                    }
                });
            },
            remove: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                swal({
                    title: "Sure to delete this user?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.delete(`/api/v1.0/users/${id}`, function (result) {
                        if (result.data) {
                            that.load();
                        } else {
                            swal('Error', 'Remove user failed', "error");
                        }
                    });
                });
            },
            update: function () {
                let that = this;
                let id = that.originItem.id;
                http.put(`/api/v1.0/users/${id}`, this.originItem, function (result) {
                    if (result.data) {
                        $('#viewModal').modal('hide');
                        that.load();
                    } else {
                        swal('Error', 'Update user failed', "error");
                    }
                });
            },
            disable: function (event) {
                let id = $(event.currentTarget).parent().parent().attr('id');
                let that = this;
                swal({
                    title: "Sure to disable this user?",
                    type: "warning",
                    showCancelButton: true
                }, function () {
                    http.put(`/api/v1.0/users/${id}/disable`, null, function () {
                        that.load();
                    });
                });
            },
            enable: function (event) {
                let that = this;
                let id = $(event.currentTarget).parent().parent().attr('id');
                http.put(`/api/v1.0/users/${id}/enable`, null, function () {
                    that.load();
                });
            }
        }
    });
});