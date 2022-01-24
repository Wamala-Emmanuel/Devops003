export function correctHeight () {
  var pageWrapper = $('#page-wrapper')
  var navbarHeigh = $('nav.navbar-default').height()
  var wrapperHeigh = pageWrapper.height()

  if (navbarHeigh > wrapperHeigh) {
    pageWrapper.css('min-height', navbarHeigh + 'px')
  }

  if (navbarHeigh < wrapperHeigh) {
    if (navbarHeigh < $(window).height()) {
      pageWrapper.css('min-height', $(window).height() + 'px')
    } else {
      pageWrapper.css('min-height', navbarHeigh + 'px')
    }
  }

  if ($('body').hasClass('fixed-nav')) {
    if (navbarHeigh > wrapperHeigh) {
      pageWrapper.css('min-height', navbarHeigh + 'px')
    } else {
      pageWrapper.css('min-height', $(window).height() - 60 + 'px')
    }
  }
}

export function detectBody () {
  if ($(document).width() < 769) {
    $('body').addClass('body-small')
  } else {
    $('body').removeClass('body-small')
  }
}
