using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfTemplate.North.UI.Common.Enums;

namespace WpfTemplate.North.UI.Common.Controls
{
	/// <summary>
	/// Interaction logic for MenuBarButton.xaml
	/// </summary>
	public partial class MenuBarButton : UserControl
	{
		public MenuBarButton()
		{
			InitializeComponent();
		}

		public Geometry IconData
		{
			get => (Geometry)GetValue(IconDataProperty);
			set => SetValue(IconDataProperty, value);
		}

		public static readonly DependencyProperty IconDataProperty =
			DependencyProperty.Register(nameof(IconData), typeof(Geometry), typeof(MenuBarButton));

		public string MenuBarButtonText
		{
			get { return (string)GetValue(MenuBarButtonTextProperty); }
			set { SetValue(MenuBarButtonTextProperty, value); }
		}

		public static readonly DependencyProperty MenuBarButtonTextProperty =
			DependencyProperty.Register(nameof(MenuBarButtonText), typeof(string), typeof(MenuBarButton), new PropertyMetadata(string.Empty));

		public Brush MenuBarButtonTextColor
		{
			get { return (Brush)GetValue(MenuBarButtonTextColorProperty); }
			set { SetValue(MenuBarButtonTextColorProperty, value); }
		}

		public static readonly DependencyProperty MenuBarButtonTextColorProperty =
			DependencyProperty.Register(nameof(MenuBarButtonTextColor), typeof(Brush), typeof(MenuBarButton),
				new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"))));

		public Brush IconFill
		{
			get { return (Brush)GetValue(IconFillProperty); }
			set { SetValue(IconFillProperty, value); }
		}

		public static readonly DependencyProperty IconFillProperty =
			DependencyProperty.Register("IconFill", typeof(Brush), typeof(MenuBarButton), new PropertyMetadata(null));

		public ICommand MenuBarButtonCommand
		{
			get { return (ICommand)GetValue(MenuBarButtonCommandProperty); }
			set { SetValue(MenuBarButtonCommandProperty, value); }
		}

		public static readonly DependencyProperty MenuBarButtonCommandProperty =
			DependencyProperty.Register("MenuBarButtonCommand", typeof(ICommand), typeof(MenuBarButton), new PropertyMetadata(null));

		//public string MenuBarButtonCommandParameter
		//{
		//	get { return (string)GetValue(MenuBarButtonCommandParameterProperty); }
		//	set { SetValue(MenuBarButtonCommandParameterProperty, value); }
		//}

		public ViewNames MenuBarButtonCommandParameter
		{
			get { return (ViewNames)GetValue(MenuBarButtonCommandParameterProperty); }
			set { SetValue(MenuBarButtonCommandParameterProperty, value); }
		}

		public static readonly DependencyProperty MenuBarButtonCommandParameterProperty =
			DependencyProperty.Register("MenuBarButtonCommandParameter", typeof(ViewNames), typeof(MenuBarButton), new PropertyMetadata(ViewNames.None));

		public Point TransformOriginValue
		{
			get { return (Point)GetValue(TransformOriginValueProperty); }
			set { SetValue(TransformOriginValueProperty, value); }
		}

		public static readonly DependencyProperty TransformOriginValueProperty =
			DependencyProperty.Register("TransformOriginValue", typeof(Point), typeof(MenuBarButton), new PropertyMetadata(new Point(0, 0)));

		public double ScaleTransformYValue
		{
			get { return (double)GetValue(ScaleTransformYValueProperty); }
			set { SetValue(ScaleTransformYValueProperty, value); }
		}

		public static readonly DependencyProperty ScaleTransformYValueProperty =
			DependencyProperty.Register("ScaleTransformYValue", typeof(double), typeof(MenuBarButton), new PropertyMetadata(1.0));

		public int ViewboxWidth
		{
			get { return (int)GetValue(ViewboxWidthProperty); }
			set { SetValue(ViewboxWidthProperty, value); }
		}

		public static readonly DependencyProperty ViewboxWidthProperty =
			DependencyProperty.Register("ViewboxWidth", typeof(int), typeof(MenuBarButton), new PropertyMetadata(25));

		public int ViewboxHeight
		{
			get { return (int)GetValue(ViewboxHeightProperty); }
			set { SetValue(ViewboxHeightProperty, value); }
		}

		public static readonly DependencyProperty ViewboxHeightProperty =
			DependencyProperty.Register("ViewboxHeight", typeof(int), typeof(MenuBarButton), new PropertyMetadata(25));

		public Thickness ButtonMargin
		{
			get { return (Thickness)GetValue(ButtonMarginProperty); }
			set { SetValue(ButtonMarginProperty, value); }
		}

		public static readonly DependencyProperty ButtonMarginProperty =
			DependencyProperty.Register("ButtonMargin", typeof(Thickness), typeof(MenuBarButton), new PropertyMetadata(new Thickness(10, 10, 0, 0)));
	}
}
