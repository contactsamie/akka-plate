﻿signalx.debug(function (o) { console.log(o); });
signalx.error(function (o) { console.log(o); });
signalx.ready(function (server) {
    var app = angular.module('AllSample', ['ngRoute']);
    angular.module('AllSample').controller('ActorsCtrl', function ($scope, $rootScope, $http, $q, $timeout) {
      
    });
    $(function () {
            var countries = [
                { Name: '', Id: 0 },
                { Name: 'United States', Id: 1 },
                { Name: 'Canada', Id: 2 },
                { Name: 'United Kingdom', Id: 3 },
                { Name: 'France', Id: 4 },
                { Name: 'Brazil', Id: 5 },
                { Name: 'China', Id: 6 },
                { Name: 'Russia', Id: 7 }
            ];

            $('#jsGrid').jsGrid({
                height: '50%',
                width: '100%',

                filtering: true,
                inserting: true,
                editing: true,
                sorting: true,
                paging: true,
                autoload: true,

                pageSize: 10,
                pageButtonCount: 5,

                deleteConfirm: 'Do you really want to delete client?',

                controller: {
                    loadData: function (filter) {
                        return $.ajax({
                            type: 'GET',
                            url: '/api/data',
                            data: filter,
                            dataType: 'json'
                        });
                    },

                    insertItem: function (item) {
                        return $.ajax({
                            type: 'POST',
                            url: '/api/data',
                            data: item,
                            dataType: 'json'
                        });
                    },

                    updateItem: function (item) {
                        return $.ajax({
                            type: 'PUT',
                            url: '/api/data/' + item.ID,
                            data: item,
                            dataType: 'json'
                        });
                    },

                    deleteItem: function (item) {
                        return $.ajax({
                            type: 'DELETE',
                            url: '/api/data/' + item.ID,
                            dataType: 'json'
                        });
                    }
                },

                fields: [
                    { name: 'Name', type: 'text', width: 150 },
                    { name: 'Age', type: 'number', width: 50, filtering: false },
                    { name: 'Address', type: 'text', width: 200 },
                    { name: 'Country', type: 'select', items: countries, valueField: 'Id', textField: 'Name' },
                    { name: 'Married', type: 'checkbox', title: 'Is Married', sorting: false },
                    { type: 'control' }
                ]
            });
        });
});