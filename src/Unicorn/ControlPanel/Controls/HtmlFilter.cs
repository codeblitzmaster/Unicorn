using System.Web.UI;

namespace Unicorn.ControlPanel.Controls
{
	internal class HtmlFilter : IControlPanelControl
	{
		public void Render(HtmlTextWriter writer)
		{
			writer.Write(@"<div class='filter-container'>
                            <div class='layer-filter-container'>
                                <h3>Filter by Layer</h3>
                                <ul class='layer-filter'></ul>
                            </div>
                            <div class='module-filter-container hidden'>
                                <h3>Filter by Module</h3>
                                <ul class='module-filter'></ul>
                            </div>
                            <div class='module-config-filter-container hidden'>
                                <h3>Filter by ModuleConfig</h3>
                                <ul class='module-config-filter'></ul>
                            </div>
                        </div>");
		}
	}
}
