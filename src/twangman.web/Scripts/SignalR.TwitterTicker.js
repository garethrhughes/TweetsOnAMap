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

        switch (parseInt(i)) {
            case 0:
            case 1:
                return rgbToHex(255, 0, 0); 
            case 2:
                return rgbToHex(255, 50, 0);
            case 3:
                return rgbToHex(255, 100, 0); 
            case 4:
                return rgbToHex(255, 150, 0); 
            case 5:
                return rgbToHex(255, 200, 0);
            case 6:
                return rgbToHex(200, 255, 0);
            case 7:
                return rgbToHex(150, 255, 0);
            case 8:
                return rgbToHex(100, 255, 0);
            case 9:
                return rgbToHex(50, 255, 0); 
            case 10:
                return rgbToHex(0, 255, 0); 
        }
    };

    var componentToHex = function (c) {
        var hex = parseInt(c).toString(16);
        return hex.length == 1 ? "0" + hex : hex;
    };

    var rgbToHex = function (r, g, b) {
        return "#" + componentToHex(r) + componentToHex(g) + componentToHex(b);
    };

    var twitterTemplate = '<li class="media navbar-inner"><button class="close pull-right">&times;</button><a class="pull-left" href="#"><img class="media-object" src="{imgUrl}"></a><div class="media-body"><a href="">@{name}</a> {tweet} </div></li>';

    $.extend(ticker.client, {
        updateUserCount: function (users) {
            $('#data-loc').html(users);
        },
        displayAccountTweet: function (text, screenName, profileImageUrl) {

            var toast = $(twitterTemplate.replace(/{imgUrl}/, profileImageUrl).replace(/{name}/, screenName).replace(/{tweet}/, text));
            toast.find('.close').click(function () {
                $(this).parents('li').fadeOut(function () {
                    $(this).remove();
                });
            });

            $('.media-list.left').prepend(toast);
            setTimeout(function () {
                toast.fadeOut(function () {
                    toast.remove();
                });
            }, 180000);
        },
        displayPostcode: function (postcode, size, rating, lat, lng, text, screenName, profileImageUrl, count) {
            $("#total-tweets").html(count);
            var toast = $(twitterTemplate.replace(/{imgUrl}/, profileImageUrl).replace(/{name}/, screenName).replace(/{tweet}/, text));
            toast.find('.close').click(function () {
                $(this).parents('li').fadeOut(function () {
                    $(this).remove();
                });
            });

            $('.media-list.right').prepend(toast);
            setTimeout(function () {
                toast.fadeOut(function () {
                    toast.remove();
                });
            }, 11000);

            var colour = colourMagic(rating);
            if (areas[postcode] == null) {
                var populationOptions = {
                    strokeColor: '#000000',
                    strokeOpacity: 0.3,
                    strokeWeight: 2,
                    fillColor: colour,
                    fillOpacity: 0.6,
                    map: map,
                    center: new google.maps.LatLng(lat, lng),
                    radius: size * 100
                };

                areas[postcode] = new google.maps.Circle(populationOptions);
            } else {
                areas[postcode].setOptions({ radius: size * 100, strokeColor: '#000000', fillColor: colour });
            }

            var infobox = new InfoBox({
                content: "<div class='infobox-inner'><img style='display: inline; float: left;' src='" + profileImageUrl + "' /><div style='padding-left: 10px;display: inline-block;float: left;'>@" + screenName + "<br />" + text + "</div></div>",
                disableAutoPan: true,
                maxWidth: 200,
                pixelOffset: new google.maps.Size(-140, 0),
                zIndex: null,
                boxStyle: {
                    background: "url('http://google-maps-utility-library-v3.googlecode.com/svn/trunk/infobox/examples/tipbox.gif') no-repeat",
                    opacity: 0.75,
                    width: "300px"
                },
                closeBoxMargin: "12px 4px 2px 2px",
                closeBoxURL: "http://www.google.com/intl/en_us/mapfiles/close.gif",
                infoBoxClearance: new google.maps.Size(1, 1)
            });

            var oldDraw = infobox.draw;
            infobox.draw = function () {
                oldDraw.apply(this);
                jQuery(infobox.div_).hide();
                jQuery(infobox.div_).fadeIn();
            }

            var oldClose = infobox.close;
            infobox.close = function () {
                jQuery(infobox.div_).fadeOut(function () {
                    jQuery(infobox.div_).hide();
                    oldClose.apply(this);
                });
            }

            infobox.open(map, {
                getPosition: function () {
                    return new google.maps.LatLng(lat, lng);
                }
            });

            setTimeout(function () {
                infobox.close();
            }, 3000);
        }
    });

    $.connection.hub.start()
    .pipe(init)
    .done(function () {

    });

});