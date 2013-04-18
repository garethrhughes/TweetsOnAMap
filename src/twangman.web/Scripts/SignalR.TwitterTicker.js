/// <reference path="/scripts/jquery-1.9.1.js" />
/// <reference path="/scripts/jquery.signalR-1.0.1.js" />

$(function () {
  var ticker = $.connection.twitterTicker;

  var init = function () {
    ticker.server.getUserCount().done(function (users) {

    });
  };

  $.extend(ticker.client, {
    updateUserCount: function (int) {
      $('#data-loc').html(int);
    }
  });

  $.connection.hub.start()
    .pipe(init)
    .done(function () {
      ticker.server.startSession();
    });

  window.onbeforeunload = function () {
    ticker.server.endSession();
  };
});