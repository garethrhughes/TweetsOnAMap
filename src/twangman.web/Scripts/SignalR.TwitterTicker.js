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

  var twitterTemplate = '<li class="media navbar-inner"><a class="pull-left" href="#"><img class="media-object" src="{imgUrl}"></a><div class="media-body"><a href="">@{name}</a> {tweet}</div></li>';

  $.extend(ticker.client, {
    updateUserCount: function (users) {
      $('#data-loc').html(users);
    },
    displayAccountTweet: function (text, screenName, profileImageUrl) {

      var toast = $(twitterTemplate.replace(/{imgUrl}/, profileImageUrl).replace(/{name}/, screenName).replace(/{tweet}/, text));
      $('.media-list.left').prepend(toast);
      setTimeout(function () {
        toast.fadeOut(function () {
          toast.remove();
        });
      }, 30000);
    },
    displayPostcode: function (postcode, size, rating, lat, lng, text, screenName, profileImageUrl) {
      var toast = $(twitterTemplate.replace(/{imgUrl}/, profileImageUrl).replace(/{name}/, screenName).replace(/{tweet}/, text));
      $('.media-list.right').prepend(toast);
      setTimeout(function () {
        toast.fadeOut(function () {
          toast.remove();
        });
      }, 11000);

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