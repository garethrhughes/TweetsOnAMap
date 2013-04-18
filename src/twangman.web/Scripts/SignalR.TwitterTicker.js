/// <reference path="/scripts/jquery-1.9.1.js" />
/// <reference path="/scripts/jquery.signalR-1.0.1.js" />

$(function () {
  var ticker = $.connection.twitterTicker;

  var init = function () {
    ticker.server.getUserCount().done(function (users) {
      $('#data-loc').html(users);
    });
  };

  $.extend(ticker.client, {
    updateUserCount: function (users) {
      $('#data-loc').html(users);
    }
  });

  $.connection.hub.start()
    .pipe(init)
    .done(function () {

    });

  window.onbeforeunload = function () {

  };
});