// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Animations
{
    /// <summary>
    /// A generic class extending <see cref="AnimationBase"/> to provide common implementation for most animations
    /// </summary>
    /// <typeparam name="T">Type of <see cref="TypedKeyFrame{U}"/> to use</typeparam>
    /// <typeparam name="U">Type of value being animated.</typeparam>
    public abstract class TypedAnimationBase<T, U> : AnimationBase
        where T : TypedKeyFrame<U>, new()
    {
        private T fromKeyFrame;
        private T toKeyFrame;

        /// <summary>
        /// Identifies the <see cref="From"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register(nameof(From), typeof(U), typeof(TypedAnimationBase<T, U>), new PropertyMetadata(GetDefaultValue(), OnAnimationPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="To"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register(nameof(To), typeof(U), typeof(TypedAnimationBase<T, U>), new PropertyMetadata(GetDefaultValue(), OnAnimationPropertyChanged));

        /// <summary>
        /// Gets or sets the value at the beginning.
        /// Setting this value adds a new <see cref="KeyFrame"/> where the Key = 0
        /// </summary>
        public U From
        {
            get { return (U)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value at the end.
        /// Setting this value generates a new <see cref="KeyFrame"/> where the Key = 1
        /// </summary>
        public U To
        {
            get { return (U)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        /// <inheritdoc/>
        public override CompositionAnimation GetCompositionAnimation(Compositor compositor)
        {
            if (DesignTimeHelpers.IsRunningInLegacyDesignerMode)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(Target))
            {
                return null;
            }

            PrepareKeyFrames();
            var animation = GetTypedAnimationFromCompositor(compositor);
            animation.Target = Target;
            animation.Duration = Duration;
            animation.DelayTime = Delay;

            if (KeyFrames.Count == 0)
            {
                animation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                return animation;
            }

            foreach (var keyFrame in KeyFrames)
            {
                if (keyFrame is T typedKeyFrame)
                {
                    InsertKeyFrameToTypedAnimation(animation, typedKeyFrame);
                }
                else if (keyFrame is ExpressionKeyFrame expressionKeyFrame)
                {
                    animation.InsertExpressionKeyFrame((float)keyFrame.Key, expressionKeyFrame.Value);
                }
            }

            return animation;
        }

        /// <summary>
        /// Creates a composition animation for the property to be animated
        /// </summary>
        /// <param name="compositor"><see cref="Compositor"/> used to create the animation</param>
        /// <returns><see cref="KeyFrameAnimation"/></returns>
        protected abstract KeyFrameAnimation GetTypedAnimationFromCompositor(Compositor compositor);

        /// <summary>
        /// Inserts the value and a specified key in the typed <see cref="KeyFrameAnimation"/>
        /// </summary>
        /// <param name="animation">The animation where the key frame will be inserted</param>
        /// <param name="keyFrame">The key frame that will be inserted</param>
        protected abstract void InsertKeyFrameToTypedAnimation(KeyFrameAnimation animation, T keyFrame);

        // these two methods are required to support double (non nullable type)
        private static object GetDefaultValue()
        {
            if (typeof(U) == typeof(double))
            {
                return double.NaN;
            }

            return default(U);
        }

        private static bool IsValueNull(U value)
        {
            if (typeof(U) == typeof(double))
            {
                return double.IsNaN((double)(object)value);
            }

            return value == null;
        }

        private void PrepareKeyFrames()
        {
            if (fromKeyFrame != null)
            {
                KeyFrames.Remove(fromKeyFrame);
            }

            if (toKeyFrame != null)
            {
                KeyFrames.Remove(toKeyFrame);
            }

            if (!IsValueNull(From))
            {
                fromKeyFrame = new T();
                fromKeyFrame.Key = 0f;
                fromKeyFrame.Value = From;
                KeyFrames.Add(fromKeyFrame);
            }

            if (!IsValueNull(To))
            {
                toKeyFrame = new T();
                toKeyFrame.Key = 1f;
                toKeyFrame.Value = To;
                KeyFrames.Add(toKeyFrame);
            }
        }
    }
}
