using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace BillSplitter
{
    // (c) Copyright Microsoft Corporation.
    // This source is subject to the Microsoft Public License (Ms-PL).
    // Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
    // All other rights reserved.
	public static class MenuFlyoutService
	{
		/// <summary>
		/// Gets the value of the MenuFlyout property of the specified object.
		/// </summary>
		/// <param name="element">Object to query concerning the MenuFlyout property.</param>
		/// <returns>Value of the MenuFlyout property.</returns>
		public static MenuFlyout GetMenuFlyout(DependencyObject element, DependencyProperty property)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			return (MenuFlyout)element.GetValue(property);
		}

		/// <summary>
		/// Sets the value of the MenuFlyout property of the specified object.
		/// </summary>
		/// <param name="element">Object to set the property on.</param>
		/// <param name="value">Value to set.</param>
		public static void SetMenuFlyout(DependencyObject element, DependencyProperty property, MenuFlyout value)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			element.SetValue(property, value);
		}

		private static void OnElementHolding(object sender, HoldingRoutedEventArgs args)
		{
			// this event is fired multiple times. We do not want to show the menu twice
			if (args.HoldingState != HoldingState.Started) return;

			FrameworkElement element = sender as FrameworkElement;

			if (element == null) return;

			// If the menu was attached properly, we just need to call this handy method
			FlyoutBase.ShowAttachedFlyout(element);
		}

		private static void OnElementTapped(object sender, TappedRoutedEventArgs args)
		{
			FrameworkElement element = sender as FrameworkElement;

			if (element == null) return;

			// If the menu was attached properly, we just need to call this handy method
			FlyoutBase.ShowAttachedFlyout(element);
		}

		/// <summary>
		/// Handles changes to the MenuFlyout DependencyProperty.
		/// </summary>
		/// <param name="o">DependencyObject that changed.</param>
		/// <param name="e">Event data for the DependencyPropertyChangedEvent.</param>
		internal static void OnMenuFlyoutChanged(DependencyObject o, DependencyPropertyChangedEventArgs e, bool onHold)
		{
			var element = o as FrameworkElement;

			if (null != element)
			{
				// just in case we were here before and there is no new menu
				if (onHold)
					element.Holding -= OnElementHolding;
				else
					element.Tapped -= OnElementTapped;

				MenuFlyout oldMenuFlyout = e.OldValue as MenuFlyout;
				if (null != oldMenuFlyout)
				{
					// Remove previous attachment
					element.SetValue(FlyoutBase.AttachedFlyoutProperty, null);
				}

				MenuFlyout newMenuFlyout = e.NewValue as MenuFlyout;
				if (null != newMenuFlyout)
				{
					// attach using FlyoutBase to easier show the menu
					element.SetValue(FlyoutBase.AttachedFlyoutProperty, newMenuFlyout);

					// need to show it
					if (onHold)
						element.Holding += OnElementHolding;
					else
						element.Tapped += OnElementTapped;
				}
			}
		}
	}


    /// <summary>
    /// Provides the system implementation for displaying a MenuFlyout.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    public static class MenuFlyoutServiceHolding
    {
        public static MenuFlyout GetMenuFlyout(DependencyObject element)
		{
			return MenuFlyoutService.GetMenuFlyout(element, MenuFlyoutProperty);
		}

		public static void SetMenuFlyout(DependencyObject element, MenuFlyout value)
		{
			MenuFlyoutService.SetMenuFlyout(element, MenuFlyoutProperty, value);
		}

		/// <summary>
		/// Identifies the MenuFlyout attached property.
		/// </summary>
		public static readonly DependencyProperty MenuFlyoutProperty = DependencyProperty.RegisterAttached(
			"MenuFlyout",
			typeof(MenuFlyout),
			typeof(MenuFlyoutService),
			new PropertyMetadata(null, OnMenuFlyoutChanged));

			private static void OnMenuFlyoutChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
				MenuFlyoutService.OnMenuFlyoutChanged(o, e, true);
		}
    }

	/// <summary>
	/// Provides the system implementation for displaying a MenuFlyout.
	/// </summary>
	/// <QualityBand>Preview</QualityBand>
	public static class MenuFlyoutServiceTapped
	{
		public static MenuFlyout GetMenuFlyout(DependencyObject element)
		{
			return MenuFlyoutService.GetMenuFlyout(element, MenuFlyoutProperty);
		}

		public static void SetMenuFlyout(DependencyObject element, MenuFlyout value)
		{
			MenuFlyoutService.SetMenuFlyout(element, MenuFlyoutProperty, value);
		}

		/// <summary>
		/// Identifies the MenuFlyout attached property.
		/// </summary>
		public static readonly DependencyProperty MenuFlyoutProperty = DependencyProperty.RegisterAttached(
			"MenuFlyout",
			typeof(MenuFlyout),
			typeof(MenuFlyoutService),
			new PropertyMetadata(null, OnMenuFlyoutChanged));

		private static void OnMenuFlyoutChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			MenuFlyoutService.OnMenuFlyoutChanged(o, e, false);
		}
	}
}


