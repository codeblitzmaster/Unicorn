using System.Web.UI;

namespace Unicorn.ControlPanel.Controls
{
	internal class HtmlFooter : IControlPanelControl
	{
		public void Render(HtmlTextWriter writer)
		{
			// this allows expanding the dependency details of a configuration when it has serialized items already
			// yes, jQuery is total overkill. yes, deal with it. :)
			writer.Write("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/1.12.0/jquery.min.js\"></script>");
			writer.Write("<script src=\"https://cdnjs.cloudflare.com/ajax/libs/js-cookie/2.1.0/js.cookie.min.js\"></script>");
			writer.Write(@"<script>
		/* Overlays */
		(function($) { 
			$.fn.overlay = function() {
				overlay = $(this);
				overlay.ready(function() {
					overlay.on('transitionend webkitTransitionEnd oTransitionEnd MSTransitionEnd', function(e) {
						if (!$(this).hasClass('shown')) return $(this).css('visibility', 'hidden');
					});
					overlay.on('show', function() {
						var $this = $(this);
						$this.css('visibility', 'visible');
						$this.addClass('shown');
						return true;
					});
					overlay.on('hide', function() {
						$(this).removeClass('shown');
						return true;
					});
					overlay.on('click', function(e) {
						if (e.target.className === $(this).attr('class')) return $(this).trigger('hide');
					})
					$('a[data-overlay-trigger=""]').on('click', function() {
						overlay.trigger('show');
					});
					
					$('a[data-modal]:not([data-modal=""])').on('click', function(e) {
						$('#' + $(this).data('modal')).trigger('show');

						e.preventDefault();
					});
				})
			};
		})(jQuery);

		jQuery(function() {
			$('.overlay').overlay();
		});

		/* Multiple Selection */
		$(function() {
			$('.fakebox:not(.fakebox-all)').on('click', function() {
				var $this = $(this);

				$this.toggleClass('checked');

				if(!$this.hasClass('checked')) $('.fakebox-all').removeClass('checked');

				UpdateBatch();
			});

			$('.fakebox-all').on('click', function() {
				var $this = $(this);

				$this.toggleClass('checked');

				var $fakeboxes = $('.fakebox:not(.fakebox-all):visible');

				if($this.hasClass('checked')) $fakeboxes.addClass('checked');
				else $fakeboxes.removeClass('checked');

				UpdateBatch();
			});
		});

		function UpdateBatch() {
			var $fakeboxes = $('.fakebox:not(.fakebox-all):visible');
			var checked = $fakeboxes.filter('.checked')
				.map(function() { return $(this).text().trim(); })
				.get();

			var allSelected = checked.length == $fakeboxes.length;
			var configSpec = checked.join('^');
			var verbosity = $('#verbosity').val();
			var skipTransparent = $('#skipTransparent').prop('checked') ? 1 : 0;

			$('.batch-sync').attr('href', '?verb=Sync&configuration=' + configSpec + '&log=' + verbosity + '&skipTransparentConfigs=' + skipTransparent);
			$('.batch-reserialize').attr('href', '?verb=Reserialize&configuration=' + configSpec + '&log=' + verbosity + '&skipTransparentConfigs=' + skipTransparent);
			$('.batch-configurations').html('<li>' + (allSelected ? 'All Configurations' :checked.join('</li><li>')) + '</li>');
			if(allSelected) $('.fakebox-all').addClass('checked');

			if(checked.length > 0) {
				$('.batch').finish().slideDown();
				$('td + td').css('visibility', 'hidden');
			}
			else {
				$('.batch').finish().slideUp(function() {
					$('td + td').css('visibility', 'visible');
				});	
			}
		}

		$(function() {
			/* Verbosity */
			var verbosityCookie = Cookies.get('UnicornLogVerbosity');
			if(verbosityCookie) {
				$('#verbosity').val(verbosityCookie);
			}

			$('#verbosity').on('change', function() {
				UpdateBatch();
				UpdateOptions();
			});

			/* Transparent Skipping */
			var transparentCookie = Cookies.get('UnicornSkipTransparent');
			$('#skipTransparent').prop('checked', transparentCookie == '1' ? true : false);

			$('#skipTransparent').on('change', function() {
				UpdateBatch();
				UpdateOptions();
			});

			UpdateOptions();

		});

		function UpdateOptions() {
			var verbosity = $('#verbosity').val();
			var skipTransparent = $('#skipTransparent').prop('checked');

			$('[data-basehref]').each(function() {
				$(this).attr('href', $(this).data('basehref') + '&log=' + verbosity + '&skipTransparentConfigs=' + skipTransparent);
			});

			Cookies.set('UnicornSkipTransparent', skipTransparent ? 1 : 0, { expires: 730 });
			Cookies.set('UnicornLogVerbosity', verbosity, { expires: 730 });
		}

        var fakeboxAll = $('.fakebox-all');
		if (fakeboxAll.offset() != null) {
        var sticky = $('.batch');
        stickyTop = fakeboxAll.offset().top - fakeboxAll.height();
        $(window).scroll(function () {
            var scroll = $(window).scrollTop();

            if (scroll >= fakeboxAll.offset().top) {
                sticky.css({ 'top': 10 });
            }
            else {
                sticky.css({ 'top': stickyTop - scroll });
            }
        });};

		var uf = {
			$layerFilter: null,
			$moduleFilter: null,
			$moduleConfigNameFilter: null,
			$filterCont: null,
			$layerFilterCont: null,
			$moduleFilterCont: null,
			$moduleConfigNameFilterCont: null,
			nodes: [],
			layers: [],
			filteredItems: [],
			checkedItems: [],
			init: function () {
				this.bindDomElements();
				if (uf.$filterCont.length > 0) {
					$('td>h3').each(function (i, el) {
						var $row = $(el).closest('tr');
						var config = $(el).text();
						var buRegEx = /([a-zA-Z]+)/g;
						var layer = config.match(buRegEx)[0];
						var module = config.match(buRegEx)[1];
						var moduleConfig = config.match(buRegEx)[2];

						// console.log(`${layer} - ${module} - ${moduleConfig}`);
						$row.attr('data-layer', layer);
						$row.attr('data-module', module);
						$row.attr('data-moduleConfig', moduleConfig);

						if (typeof (layer) != 'undefined')
							uf.layers.push(layer);


						uf.nodes.push({ config: $(el).text(), $node: $row, layer: layer, module: module, moduleConfig: moduleConfig, filtered: false });
					});

					uf.layers = uf.layers.filter(function (v, i) { return uf.layers.indexOf(v) == i; });

					uf.layers.forEach(function (name) {
						uf.AddFilterOption('layer', name);
					});

					this.bindFilterEvent();
				}
			},
			bindDomElements: function () {
				uf.$filterCont = $('.filter-container');
				uf.$layerFilter = $('.layer-filter');
				uf.$moduleFilter = $('.module-filter');
				uf.$moduleConfigNameFilter = $('.module-config-filter');
				uf.$layerFilterCont = $('.layer-filter-container');
				uf.$moduleFilterCont = $('.module-filter-container');
				uf.$moduleConfigNameFilterCont = $('.module-config-filter-container');
			},
			bindFilterEvent: function () {
				$(document).on('click', '.filter-item', function () {
					var $this = $(this);

					$this.toggleClass('checked');
					var isAddEvent = $this.hasClass('checked');
					var currentQuery = uf.getCurrentFilterAttributes($this);

					uf.checkedItems = [];
					$('.filter-item.checked').each(function (i, el) {
						uf.checkedItems.push(uf.getCurrentFilterAttributes($(el), true))
					});

					uf.filteredItems = uf.checkedItems.reduce(function (filtered, x) {
						switch (x.type) {
							case 'module':
								if (!isAddEvent && x.parentQuery == currentQuery) {
									filtered = filtered.filter(function (query) { return query != x.query });
								}
								else {
									filtered = filtered.filter(function (query) { return query != x.parentQuery });
								}
								break;
							case 'moduleConfig':
								if (!isAddEvent && x.parentQuery == currentQuery) {
									filtered = filtered.filter(function (query) { return query != x.query });
								}
								else {
									filtered = filtered.filter(function (query) { return query != x.parentQuery });
								}
								break;
						}
						return filtered;
					}, uf.checkedItems.map(function (x) { return x.query }));
					// console.log(uf.filteredItems);

					var type = $this.attr('data-type');

					var filterName = $this.attr('data-' + type);
					var parentFilterName = type == 'module' ? $this.attr('data-layer') : $this.attr('data-module');

					var $filter = uf.getFilterByType(type);
					var hasAtleastOneChecked = $filter.find('.checked').length;;

					var childFilterType = uf.getChildFilterType(type);
					var $filterToToggle = uf.getFilterContainerByType(type != 'moduleConfig' ? childFilterType : type);

					if ($this.hasClass('checked')) {
						if (type == 'layer' || type == 'module') {
							uf.renderSubFilter(type, filterName, parentFilterName);
							if (hasAtleastOneChecked >= 1)
								$filterToToggle.show();
						}
					}
					else {

						var cond = uf.getCurrentFilterAttributes($(this));
						if (type != 'moduleConfig') {
							var filteredItems = $filterToToggle.find(cond);
							filteredItems.remove();
						}

						if (type == 'layer') {
							var filteredItems = uf.$moduleConfigNameFilterCont.find(cond);
							filteredItems.remove();
						}
						if (hasAtleastOneChecked < 1 && uf.$moduleConfigNameFilterCont.find('.filter-item').length < 1) {
							$filterToToggle.hide();
						}
					}

					if (uf.filteredItems.length > 0) {
						var filterEls = uf.filteredItems.join();
						$('tr:not(' + filterEls + ')').hide();
						$(filterEls).show();
					}
					else {
						$('tr').show();
					}
				});
			},
			getFilterByType: function (type) {
				switch (type) {
					case 'layer':
						return uf.$layerFilter;
						break;
					case 'module':
						return uf.$moduleFilter;
						break;

					case 'moduleConfig':
						return uf.$moduleConfigNameFilter;
						break;
				}
			},
			getFilterContainerByType: function (type) {
				switch (type) {
					case 'layer':
						return uf.$layerFilterCont;
						break;
					case 'module':
						return uf.$moduleFilterCont;
						break;

					case 'moduleConfig':
						return uf.$moduleConfigNameFilterCont;
						break;
				}
			},
			getChildFilterType: function (type) {
				switch (type) {
					case 'layer':
						return 'module';
						break;
					case 'module':
						return 'moduleConfig';
						break;

					case 'moduleConfig':
					default:
				}
			},
			AddFilterOption: function (type, name, level1 = null, level2 = null) {
				if (typeof (name) != 'undefined') {

					var $filterType = this.getFilterByType(type);

					var filterItemClass = 'filter-item';

					var $li;
					if (level1 == null) {
						$li = $('<li class='' + filterItemClass + '' data-type='' + type + '' data-' + type + '='' + name + ''/>');
						$li.append('<span></span>' + name);
					}
					else {
						if (level2 == null) {
							//module
							$li = $('<li class='' + filterItemClass + '' data-type='' + type + ''  data-layer='' + level1 + '' data-' + type + '='' + name + ''/>');
							$li.append('<span></span>' + level1 + '.' + name);
						}
						else {
							//moduleConfig
							$li = $('<li class='' + filterItemClass + '' data-type='' + type + ''  data-layer='' + level2 + '' data-module='' + level1 + '' data-' + type + '='' + name + ''/>');
							$li.append('<span></span>' + level1 + '.' + name);
						}

					}
					$filterType.append($li);
				}
			},
			renderSubFilter: function (type, filterName, parentFilterName) {
				var filteredUnicornConfigs = uf.nodes.reduce(function (filtered, x) {
					switch (type) {
						case 'layer':
							if (x.layer == filterName) {
								filtered.push(x.module);
							}
							break;
						case 'module':
							if (x.module == filterName) {
								filtered.push(x.moduleConfig);
							}
							break;
						case 'moduleConfig':
							break;
					}

					return filtered.filter(function (v, i) { return filtered.indexOf(v) == i; });
				}, []);


				var childFilterType = uf.getChildFilterType(type);

				filteredUnicornConfigs.forEach(function (name) {
					uf.AddFilterOption(childFilterType, name, filterName, parentFilterName);
				});
			},
			getCurrentFilterAttributes: function ($this, asObj = false) {
				var type = $this.attr('data-type');
				var layer = $this.attr('data-layer');
				var module = $this.attr('data-module');
				var moduleConfig = $this.attr('data-moduleConfig');
				var obj = {
					type: type,
					layer: layer,
					module: module,
					moduleConfig: moduleConfig,
					query: '',
					parentQuery: ''
				};

				var data = '[data-layer='' + obj.layer + '']'
				if (obj.type == 'module') {
					obj.parentQuery = data;
					data += '[data-module='' + obj.module + '']';
				}
				else if (obj.type == 'moduleConfig') {
					data += '[data-module='' + obj.module + '']';
					obj.parentQuery = data;
					data += '[data-moduleConfig='' + obj.moduleConfig + '']';
				}
				obj.query = data;
				return asObj ? obj : data;
			}
		};

		function escapeRegExp(string) {
			return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
		}
	</script>");
			writer.Write(" </body></html>");
		}
	}
}
