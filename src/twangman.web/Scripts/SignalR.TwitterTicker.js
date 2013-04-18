/// <reference path="/scripts/jquery-1.9.1.js" />
/// <reference path="/scripts/jquery.signalR-1.0.1.js" />

$(function () {
    var ticker = $.connection.twitterTicker;
    var areas = {};

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
        var hex = parseInt(c).toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    };

    var rgbToHex = function (r, g, b) {
        return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
    };


    $.extend(ticker.client, {
        updateUserCount: function (users) {
            $('#data-loc').html(users);
        },
        displayPostcode: function (postcode, size, rating, lat, lng, text, screenName, profileImageUrl) {
            $('#postcode').html(postcode + " " + rating + " " + text);
            var colour = colourMagic(rating);
            if (areas[postcode] == null) {
                var populationOptions = {
                    strokeColor: colour,
                    strokeOpacity: 0.8,
                    strokeWeight: 2,
                    fillColor: colour,
                    fillOpacity: 0.6,
                    map: map,
                    center: new google.maps.LatLng(lat, lng),
                    radius: size * 100
                };

                areas[postcode] = new google.maps.Circle(populationOptions);
            } else {
                areas[postcode].setOptions({ radius: size * 100, strokeColor: colour, fillColor: colour });
            }

            var coordInfoWindow = new google.maps.InfoWindow({
                content: "<img style='display: inline; float: left;' src='" + profileImageUrl + "' /><div style='padding-left: 10px;display: inline-block;float: left;'>@" + screenName + "<br />" + text + "</div>",
                disableAutoPan: true,
                position: new google.maps.LatLng(lat, lng)
            });

            coordInfoWindow.open(map);
            
            setTimeout(function () {
                coordInfoWindow.close();
            }, 5000);
        }
    });

    $.connection.hub.start()
    .pipe(init)
    .done(function () {

    });

    window.onbeforeunload = function () {

    };
});