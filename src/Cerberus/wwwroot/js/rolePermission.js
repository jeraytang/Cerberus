$(function () {
    new Vue({
        el: '#app',
        data: {
            role: '',
            items: [],
        },
        mounted: function () {
            this.load();
        },
        methods: {
            load: function () {
                let that = this;
                let id = this.getId();
                http.get(`/api/v1.0/roles/${id}/permissions`, function (result) {
                    that.role = result.data.role;
                    that.items = [];
                    result.data.services.forEach(x => {
                        that.items.push(x);
                    });
                });
            },
            getId: function () {
                let data = window.location.href.split('/');
                return data[data.length - 2];
            },
            permissionChange: function (event) {
                let that = this;
                let id = this.getId();
                let checked = event.target.checked;
                let permissionId = $(event.target).attr('id');
                if (checked) {
                    http.post(`/api/v1.0/roles/${id}/permissions/${permissionId}`, null, function () {
                        that.items.forEach(x => {
                            if (x.id === permissionId) {
                                x.hasPermission = true;
                            }
                        })
                    });
                } else {
                    http.delete(`/api/v1.0/roles/${id}/permissions/${permissionId}`, function () {
                        that.items.forEach(x => {
                            if (x.id === permissionId) {
                                x.hasPermission = false;
                            }
                        })
                    });
                }
            }
        }
    });
});