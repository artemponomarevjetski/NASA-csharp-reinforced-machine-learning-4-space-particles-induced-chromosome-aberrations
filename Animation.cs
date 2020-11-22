using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;

namespace GraficDisplay
{
    public partial class Radiation
    {
        public void Movie()
        {
            // Create a red rectangle that will be the target
            // of the animation.
            System.Windows.Shapes.Rectangle myRectangle = new System.Windows.Shapes.Rectangle();
            myRectangle.Width = 200;
            myRectangle.Height = 200;
            Color myColor = Color.FromArgb(255, 255, 0, 0);
            SolidColorBrush myBrush = new SolidColorBrush();
            myBrush.Color = myColor;
            myRectangle.Fill = myBrush;
            //
            // Add the rectangle to the tree.
            System.Windows.Controls.Canvas LayoutRoot = new System.Windows.Controls.Canvas();
            LayoutRoot.Children.Add(myRectangle);
            //
            // Create two DoubleAnimations and set their properties.
            DoubleAnimation myDoubleAnimation1 = new DoubleAnimation();
            DoubleAnimation myDoubleAnimation2 = new DoubleAnimation();
            //
            // Create a duration of 2 seconds.
            myDoubleAnimation1.Duration = TimeSpan.FromSeconds(0.2);
            myDoubleAnimation2.Duration = TimeSpan.FromSeconds(0.2);
            //
            Storyboard sb = new Storyboard();
            sb.Duration = TimeSpan.FromSeconds(0.2);
            //
            sb.Children.Add(myDoubleAnimation1);
            sb.Children.Add(myDoubleAnimation2);
            //
            Storyboard.SetTarget(myDoubleAnimation1, myRectangle);
            Storyboard.SetTarget(myDoubleAnimation2, myRectangle);
            //
            // Set the attached properties of Canvas.Left and Canvas.Top
            // to be the target properties of the two respective DoubleAnimations
            Storyboard.SetTargetProperty(myDoubleAnimation1, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTargetProperty(myDoubleAnimation2, new PropertyPath(Canvas.TopProperty));
            //
            myDoubleAnimation1.To = 200;
            myDoubleAnimation2.To = 200;
            //
            // Make the Storyboard a resource.
            LayoutRoot.Resources.Add("sb", sb);
            //
            // Begin the animation.
            sb.Begin();
        }
    }
}

//public void Designer()
//{
//    Control_ = new UserControl();

//    Control_.HorizontalAlignment = HorizontalAlignment.Stretch;
//    Control_.VerticalAlignment = VerticalAlignment.Stretch;

//    Control_.Name = "Control_";
//    this.AddChild(Control_);

//    CreateStoryboard();
//}

//public void CreateStoryboard()
//{
//    fadeinBoard = new Storyboard();
//    Duration duration = new Duration(TimeSpan.FromMilliseconds(5));
//    fadeinBoard.Duration = duration;

//    DoubleAnimationUsingKeyFrames animOpacity = new DoubleAnimationUsingKeyFrames();
//    DoubleAnimationUsingKeyFrames animTransform = new DoubleAnimationUsingKeyFrames();

//    animOpacity.Duration = duration;
//    animTransform.Duration = duration;

//    //Transform Function
//    KeyTime ktime1 = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.5));
//    PowerEase pow = new PowerEase();
//    pow.Power = 5;
//    pow.EasingMode = EasingMode.EaseOut;
//    EasingDoubleKeyFrame keyFrame1 = new EasingDoubleKeyFrame(0, ktime1, pow);

//    //Opacity Function
//    KeyTime ktime2 = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(.5));
//    ExponentialEase expo = new ExponentialEase();
//    expo.Exponent = 3;
//    expo.EasingMode = EasingMode.EaseOut;
//    EasingDoubleKeyFrame keyFrame2 = new EasingDoubleKeyFrame(1, ktime2, expo);

//    animOpacity.KeyFrames.Add(keyFrame1);
//    animTransform.KeyFrames.Add(keyFrame2);

//    // MY PROBLEM IS HERE
//    //Storyboard.SetTarget(???, ???);
//    //Storyboard.SetTarget(???, ???);
//    //Storyboard.SetTargetProperty(???, ???);
//    //Storyboard.SetTargetProperty(???, ???)));

//    fadeinBoard.Children.Add(animOpacity);
//    fadeinBoard.Children.Add(animTransform);

//    Control_.Resources.Add("fader", fadeinBoard);
//}
