/// <reference path="/scripts/jquery-1.9.1.js" />
/// <reference path="/scripts/jquery.signalR-1.0.1.js" />

$(function () {
    var ticker = $.connection.twitterTicker;

    var init = function () {
        ticker.server.getUserCount().done(function (users) {
            $('#data-loc').html(users);
        });
    };

    var colourMagic = function (i) {
        var j = i * 10;
        var R = (255 * j) / 100;
        var G = (255 * (100 - j)) / 100;

        return rgbToHex(G, R, 0);
    };

    var componentToHex = function (c) {
        var hex = c.toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    };

    var rgbToHex = function(r, g, b) {
        return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
    };

    $.extend(ticker.client, {
        updateUserCount: function (users) {
            $('#data-loc').html(users);
        },
        displayPostcode: function (postcode, size, rating, lat, lng) {
            $('#postcode').html(postcode + " " + rating);

            var populationOptions = {
                strokeColor: colourMagic(rating),
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: colourMagic(rating),
                fillOpacity: 0.35,
                map: map,
                center: new google.maps.LatLng(lat, lng),
                radius: size
            };
            console.log(map, lat, lng);
            new google.maps.Circle(populationOptions);
        }
    });

    $.connection.hub.start()
    .pipe(init)
    .done(function () {

    });

    window.onbeforeunload = function () {

    };
});