$(function () {
    new Vue({
        el: '#app',
        data: {
            userName: '',
            name: '',
            email: '',
            items: [],
        },
        mounted: function () {
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let id = this.getId();
                http.get(`/api/v1.0/users/${id}/roles`, function (result) {
                    that.userName = result.data.userName;
                    that.name = result.data.name;
                    that.email = result.data.email;
                    that.items = [];
                    result.data.groups.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            getId: function () {
                let data = window.location.href.split('/');
                return data[data.length - 2];
            },
            roleChange: function (event) {
                let that = this;
                let id = this.getId();
                let checked = event.target.checked;
                let roleId = $(event.target).attr('id');
                if (checked) {
                    http.post(`/api/v1.0/users/${id}/roles/${roleId}`, null, function (result) {
                        that.items.forEach(x => {
                            if (x.id === roleId) {
                                x.inRole = true;
                            }
                        })
                    });
                } else {
                    http.delete(`/api/v1.0/users/${id}/roles/${roleId}`, function () {
                        that.items.forEach(x => {
                            if (x.id === roleId) {
                                x.inRole = false;
                            }
                        })
                    });
                }
            }
        }
    });
});