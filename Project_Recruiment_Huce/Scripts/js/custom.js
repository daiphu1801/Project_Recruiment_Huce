jQuery(function ($) {

	'use strict';

	$(".loader").delay(1000).fadeOut("slow");
	$("#overlayer").delay(1000).fadeOut("slow");

	var siteMenuClone = function () {

		$('.js-clone-nav').each(function () {
			var $this = $(this);
			$this.clone().attr('class', 'site-nav-wrap').appendTo('.site-mobile-menu-body');
		});


		setTimeout(function () {

			var counter = 0;
			$('.site-mobile-menu .has-children').each(function () {
				var $this = $(this);

				$this.prepend('<span class="arrow-collapse collapsed">');

				$this.find('.arrow-collapse').attr({
					'data-toggle': 'collapse',
					'data-target': '#collapseItem' + counter,
				});

				$this.find('> ul').attr({
					'class': 'collapse',
					'id': 'collapseItem' + counter,
				});

				counter++;

			});

		}, 1000);

		$('body').on('click', '.arrow-collapse', function (e) {
			var $this = $(this);
			if ($this.closest('li').find('.collapse').hasClass('show')) {
				$this.removeClass('active');
			} else {
				$this.addClass('active');
			}
			e.preventDefault();

		});

		$(window).resize(function () {
			var $this = $(this),
				w = $this.width();

			if (w > 768) {
				if ($('body').hasClass('offcanvas-menu')) {
					$('body').removeClass('offcanvas-menu');
				}
			}
		})

		// Mobile menu toggle handler
		var menuToggleHandler = function (e) {
			if (e) {
				e.preventDefault();
				e.stopPropagation();
				e.stopImmediatePropagation();
			}

			var $body = $('body');
			var isOpen = $body.hasClass('offcanvas-menu');

			if (isOpen) {
				$body.removeClass('offcanvas-menu');
				$('.js-menu-toggle').removeClass('active');
			} else {
				// Đảm bảo menu content đã được clone
				if ($('.site-mobile-menu-body .site-nav-wrap').length === 0) {
					$('.js-clone-nav').each(function () {
						var $clone = $(this).clone();
						$clone.attr('class', 'site-nav-wrap').find('.d-none').removeClass('d-none');
						$('.site-mobile-menu-body').append($clone);
					});
				}
				$body.addClass('offcanvas-menu');
				$('.js-menu-toggle').addClass('active');
			}
			return false;
		};

		// Đăng ký event handlers
		$(document).on('click', '.js-menu-toggle', menuToggleHandler);

		// Touch events cho mobile
		setTimeout(function () {
			document.querySelectorAll('.js-menu-toggle').forEach(function (element) {
				element.style.cssText += 'cursor: pointer; pointer-events: auto; -webkit-tap-highlight-color: rgba(0,0,0,0.1);';
				element.addEventListener('touchstart', function (e) { e.stopPropagation(); }, { passive: true, capture: true });
				element.addEventListener('touchend', function (e) {
					e.preventDefault();
					e.stopPropagation();
					e.stopImmediatePropagation();
					menuToggleHandler.call(this, e);
					return false;
				}, { passive: false, capture: true });
				element.addEventListener('mousedown', function (e) { e.stopPropagation(); }, { capture: true });
			});
		}, 100);

		// Ngăn các element ở header đóng mobile menu - chỉ khi mobile menu đang mở
		$(document).on('click', '.right-cta-menu, .right-cta-menu a, .right-cta-menu button, .right-cta-menu .dropdown, .right-cta-menu .dropdown-toggle, .right-cta-menu .dropdown-menu, .right-cta-menu .dropdown-item, .site-navbar a, .site-navbar button', function (e) {
			// Chỉ xử lý nếu không nằm trong mobile menu và mobile menu đang mở
			if ($(this).closest('.site-mobile-menu').length === 0 && $('body').hasClass('offcanvas-menu')) {
				e.stopPropagation(); // Ngăn event lan truyền đến document click handler
			}
		});

		// click outside offcanvas - chỉ đóng menu khi click bên ngoài menu và header
		$(document).on('click', function (e) {
			// Chỉ xử lý nếu mobile menu đang mở
			if (!$('body').hasClass('offcanvas-menu')) {
				return;
			}

			var container = $(".site-mobile-menu");
			var $target = $(e.target);

			// Bỏ qua nếu click vào header (right-cta-menu, site-navbar) - bao gồm tất cả button và link
			if (($target.closest('.right-cta-menu').length > 0 || $target.closest('.site-navbar').length > 0) &&
				$target.closest('.site-mobile-menu').length === 0) {
				return false; // Không đóng menu khi click vào header
			}

			// Bỏ qua nếu click vào menu toggle button
			if ($target.closest('.js-menu-toggle').length > 0 || $target.hasClass('js-menu-toggle')) {
				return false; // Không đóng menu khi click vào toggle button
			}

			// Chỉ đóng menu nếu click bên ngoài mobile menu và header
			if (!container.is(e.target) && container.has(e.target).length === 0) {
				$('body').removeClass('offcanvas-menu');
				$('.js-menu-toggle').removeClass('active');
			}
		});
	};
	siteMenuClone();


	var sitePlusMinus = function () {
		$('.js-btn-minus').on('click', function (e) {
			e.preventDefault();
			if ($(this).closest('.input-group').find('.form-control').val() != 0) {
				$(this).closest('.input-group').find('.form-control').val(parseInt($(this).closest('.input-group').find('.form-control').val()) - 1);
			} else {
				$(this).closest('.input-group').find('.form-control').val(parseInt(0));
			}
		});
		$('.js-btn-plus').on('click', function (e) {
			e.preventDefault();
			$(this).closest('.input-group').find('.form-control').val(parseInt($(this).closest('.input-group').find('.form-control').val()) + 1);
		});
	};
	// sitePlusMinus();

	var siteIstotope = function () {
		/* activate jquery isotope */
		var $container = $('#posts').isotope({
			itemSelector: '.item',
			isFitWidth: true
		});

		$(window).resize(function () {
			$container.isotope({
				columnWidth: '.col-sm-3'
			});
		});

		$container.isotope({ filter: '*' });

		// filter items on button click
		$('#filters').on('click', 'button', function (e) {
			e.preventDefault();
			var filterValue = $(this).attr('data-filter');
			$container.isotope({ filter: filterValue });
			$('#filters button').removeClass('active');
			$(this).addClass('active');
		});
	}

	siteIstotope();

	var fancyBoxInit = function () {
		$('.fancybox').on('click', function () {
			var visibleLinks = $('.fancybox');

			$.fancybox.open(visibleLinks, {}, visibleLinks.index(this));

			return false;
		});
	}
	fancyBoxInit();


	var stickyFillInit = function () {
		$(window).on('resize orientationchange', function () {
			recalc();
		}).resize();

		function recalc() {
			if ($('.jm-sticky-top').length > 0) {
				var elements = $('.jm-sticky-top');
				Stickyfill.add(elements);
			}
		}
	}
	stickyFillInit();


	// Helper function để đóng mobile menu
	var closeMobileMenu = function () {
		$('body').removeClass('offcanvas-menu');
		$('.js-menu-toggle').removeClass('active');
	};

	// Helper function để kiểm tra dropdown
	var isDropdown = function ($link) {
		return $link.closest('.dropdown').length > 0 ||
			$link.hasClass('dropdown-toggle') ||
			$link.hasClass('dropdown-item') ||
			$link.attr('data-toggle') === 'dropdown' ||
			$link.attr('aria-haspopup') === 'true';
	};

	// navigation - xử lý anchor links (scroll to section)
	var OnePageNavigation = function () {

		// Xử lý anchor links trong main menu và smoothscroll
		$("body").on("click", ".main-menu li a[href^='#'], .smoothscroll[href^='#']", function (e) {
			var $link = $(this);
			if (isDropdown($link)) return true;

			var hash = this.hash;
			var href = this.getAttribute('href') || '';
			if (!href || href === '#' || href.trim() === '#' || !hash || hash === '#') {
				return true;
			}

			var $target = $(hash);
			if (!$target.length) return true;

			var targetOffset = $target.offset();
			if (!targetOffset || typeof targetOffset.top === 'undefined') {
				return true;
			}

			e.preventDefault();
			$('html, body').animate({
				'scrollTop': targetOffset.top
			}, 600, 'easeInOutCirc', function () {
				window.location.hash = hash;
			});
			return false;
		});

		// Xử lý riêng cho mobile menu
		$("body").on("click", ".site-mobile-menu .site-nav-wrap li a", function (e) {
			var $link = $(this);
			var href = this.getAttribute('href') || '';
			var hash = this.hash || '';

			// Bỏ qua dropdown toggles
			if (isDropdown($link) || !href || href === '#' || href.trim() === '#') {
				if ($link.hasClass('dropdown-item')) {
					closeMobileMenu();
				}
				return true;
			}

			// Nếu không phải anchor link, đóng menu và navigate
			var isAnchorLink = href.trim().charAt(0) === '#';
			if (!isAnchorLink || !hash || hash === '#') {
				closeMobileMenu();
				return true;
			}

			// Kiểm tra element và offset
			var $target = $(hash);
			if (!$target.length) {
				closeMobileMenu();
				return true;
			}

			var targetOffset = $target.offset();
			if (!targetOffset || typeof targetOffset.top === 'undefined' || isNaN(targetOffset.top)) {
				closeMobileMenu();
				return true;
			}

			// Scroll đến anchor link
			e.preventDefault();
			e.stopPropagation();
			closeMobileMenu();

			$('html, body').animate({
				'scrollTop': targetOffset.top
			}, 600, 'easeInOutCirc', function () {
				window.location.hash = hash;
			});
			return false;
		});
	};
	OnePageNavigation();

	var counterInit = function () {
		if ($('.section-counter').length > 0) {
			$('.section-counter').waypoint(function (direction) {

				if (direction === 'down' && !$(this.element).hasClass('ftco-animated')) {

					var comma_separator_number_step = $.animateNumber.numberStepFactories.separator(',')
					$('.number').each(function () {
						var $this = $(this);
						$this.animateNumber({
							number: $this.data('number'),
							numberStep: comma_separator_number_step
						}, 7000);
					});

				}

			}, { offset: '95%' });
		}

	}
	counterInit();

	var selectPickerInit = function () {
		$('.selectpicker').selectpicker();
	}
	selectPickerInit();

	var owlCarouselFunction = function () {
		$('.single-carousel').owlCarousel({
			loop: true,
			margin: 0,
			nav: true,
			autoplay: true,
			items: 1,
			nav: false,
			smartSpeed: 1000
		});

	}
	owlCarouselFunction();

	var quillInit = function () {

		var toolbarOptions = [
			['bold', 'italic', 'underline', 'strike'],        // toggled buttons
			['blockquote', 'code-block'],

			[{ 'header': 1 }, { 'header': 2 }],               // custom button values
			[{ 'list': 'ordered' }, { 'list': 'bullet' }],
			[{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
			[{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
			[{ 'direction': 'rtl' }],                         // text direction

			[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
			[{ 'header': [1, 2, 3, 4, 5, 6, false] }],

			[{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
			[{ 'font': [] }],
			[{ 'align': [] }],

			['clean']                                         // remove formatting button
		];

		if ($('.editor').length > 0) {
			var quill = new Quill('#editor-1', {
				modules: {
					toolbar: toolbarOptions,
				},
				placeholder: 'Compose an epic...',
				theme: 'snow'  // or 'bubble'
			});
			var quill = new Quill('#editor-2', {
				modules: {
					toolbar: toolbarOptions,
				},
				placeholder: 'Compose an epic...',
				theme: 'snow'  // or 'bubble'
			});
		}

	}
	quillInit();

});