using Microsoft.Maui.Controls;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
#else
using PlatformView = System.Object;
#endif

namespace OWCE.Behaviors
{
    // CommunityToolkit.Maui behaviors don't inherit the host view's BindingContext
    // (Xamarin's TouchEffect did), so plain {Binding SomeCommand} on a TouchBehavior
    // resolves to nothing and taps are ignored. This follows the host's BindingContext.
    public class TouchBehavior : CommunityToolkit.Maui.Behaviors.TouchBehavior
    {
        protected override void OnAttachedTo(VisualElement bindable, PlatformView platformView)
        {
            SetBinding(BindingContextProperty, new Binding(nameof(BindingContext), source: bindable));
            base.OnAttachedTo(bindable, platformView);
        }

        protected override void OnDetachedFrom(VisualElement bindable, PlatformView platformView)
        {
            base.OnDetachedFrom(bindable, platformView);
            RemoveBinding(BindingContextProperty);
        }
    }
}
