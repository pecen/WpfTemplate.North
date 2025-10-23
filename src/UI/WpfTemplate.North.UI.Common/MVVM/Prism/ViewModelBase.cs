using Prism.Commands;
using Prism.Mvvm;

namespace WpfTemplate.North.UI.Common.MVVM.Prism
{
	public class ViewModelBase : BindableBase
	{
		public DelegateCommand<object>? NavigateCommand { get; set; }

		private string? _title;
		public string? Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}
	}
}
