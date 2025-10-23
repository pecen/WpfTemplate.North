using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using WpfTemplate.North.UI.Shell.Views;

namespace WpfTemplate.North.UI.Shell
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : PrismApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{

		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
		}
	}
}
