namespace AkkaBootCampThings
{
    public class UIClass
    {
        public static string UI = @"<!DOCTYPE html>
							<html>
                            <head>
<style>
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

html {
    height: 100%;
}

body {
    height: 100%;
    color: #262626;
    font-family: 'Helvetica Neue Light', 'Open Sans', Helvetica;
    font-size: 14px;
    font-weight: 300;
    padding: 1px;
}

h1 {
    margin: 0 0 8px 0;
    font-size: 24px;
    font-family: 'Helvetica Neue Light', 'Open Sans', Helvetica;
    font-weight: 300;
}

h2 {
    margin: 16px 0 8px 0;
    font-size: 18px;
    font-family: 'Helvetica Neue Light', 'Open Sans', Helvetica;
    font-weight: 300;
}

ul {
    list-style: none;
}

a {
    color: #2ba6cb;
    text-decoration: none;
}

a:hover {
    text-decoration: underline;
    color: #258faf;
}

input, button, select {
    font-family: 'Helvetica Neue Light', 'Open Sans', Helvetica;
    font-weight: 300;
    font-size: 14px;
    padding: 2px;
}
</style>
                            <script src='https://ajax.aspnetcdn.com/ajax/jquery/jquery-1.9.0.min.js'></script>
							<script src='https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.js'></script>
						    <script src='https://unpkg.com/signalx'></script>
                            <link href='http://fonts.googleapis.com/css?family=Open+Sans:300,600,400' rel='stylesheet' type='text/css'>
                            <script src='https://ajax.googleapis.com/ajax/libs/angularjs/1.5.6/angular.min.js'></script>
                            <script src='https://ajax.googleapis.com/ajax/libs/angularjs/1.5.6/angular-route.js'></script>
<link rel='stylesheet' href='//code.jquery.com/ui/1.11.2/themes/cupertino/jquery-ui.css'>
                            <link type = 'ext/css' rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid.min.css' />
                            <link type = 'text/css' rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid-theme.min.css' />
                            <script type = 'text/javascript' src='https://cdnjs.cloudflare.com/ajax/libs/jsgrid/1.5.1/jsgrid.min.js'></script>
                            </head>
							<body>
							<div  ng-controller='ActorsCtrl'>
							    <input ng-model='inp' type='text'/>
							    <button ng-click='inp=inp+1'>Send Message To Server</button>
                            </div>
                            <div id='jsGrid'></div>

							<script>
							    signalx.debug(function (o) { console.log(o); });
                                signalx.error(function (o) { console.log(o); });   
                                signalx.client('update',function(response){ 
                                    console.log('Inside information'); 
                                    console.log(response);
                                });                             
                                signalx.ready(function (server) {
                                var app = angular.module('all', ['ngRoute']);

                                app.controller('ActorsCtrl', function ($scope, $rootScope, $http, $q, $timeout) {
                                });
                                angular.element(function() {
                                        angular.bootstrap(document, ['all']);
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

                                            $( document ).ajaxComplete(function() {
                                                     signalx.server.update && signalx.server.update('I have made an update',function(response){ 
                                                        console.log(response); 
                                                        $( document ).append(response);
                                                    });
                                            });

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
                                                            url:'http://localhost:8018/api/data/get/',
                                                             data:  JSON.stringify(filter),
                                                            dataType: 'json',
                                                            contentType: 'application/json'
                                                        });
                                                    },

                                                    insertItem: function (item) {
                                                        return $.ajax({
                                                            type: 'POST',
                                                            url: 'http://localhost:8018/api/data/post/',
                                                             data:  JSON.stringify(item),
                                                            dataType: 'json',
                                                            contentType: 'application/json'
                                                        });
                                                    },

                                                    updateItem: function (item) {
                                                        return $.ajax({
                                                            type: 'PUT',
                                                            url: 'http://localhost:8018/api/data/put/'+ item.ID,
                                                            data:  JSON.stringify(item),
                                                            dataType: 'json',
                                                            contentType: 'application/json'
                                                        });
                                                    },

                                                    deleteItem: function (item) {
                                                        return $.ajax({
                                                            type: 'DELETE',
                                                            url: 'http://localhost:8018/api/data/delete/' + item.ID,
                                                            dataType: 'json',
                                                            contentType: 'application/json'
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
							</script>

							</body>
							</html>";
    }
}