// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Rendering.Composers;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Base implementation for a <see cref="ISceneRenderer"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class SceneRendererBase : RendererBase, ISceneRenderer
    {
        protected SceneRendererBase()
        {
            Output = new CurrentRenderFrameProvider();
            Parameters = new ParameterCollection();
            ResetGraphicsStates = true;
        }

        /// <summary>
        /// Gets or sets the output of the scene renderer
        /// </summary>
        /// <userdoc>Specify the render frame to use as output of the scene renderer</userdoc>
        [DataMember(100)]
        [NotNull]
        public ISceneRendererOutput Output { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to reset the graphics states after this scene renderer is executed.
        /// </summary>
        /// <value><c>true</c> to reset the graphics states after this scene renderer is executed.; otherwise, <c>false</c>.</value>
        /// <userdoc>If this option is selected, the graphics states (blend, depth...etc.) are reseted after this scene renderer is executed.</userdoc>
        [Display("Reset Graphics States?")]
        [DataMember(110)]
        [DefaultValue(true)]
        public bool ResetGraphicsStates { get; set; }

        /// <summary>
        /// Gets the parameters used to in place of the default <see cref="RenderContext.Parameters"/>.
        /// </summary>
        /// <value>The parameters.</value>
        [DataMemberIgnore]
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets the current output <see cref="RenderFrame"/> output.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>RenderFrame.</returns>
        public RenderFrame GetOutput(RenderContext context)
        {
            return Output.GetSafeRenderFrame(context);
        }

        /// <summary>
        /// Activates the output to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="disableDepth">if set to <c>true</c> [disable depth].</param>
        public void ActivateOutput(RenderContext context, bool disableDepth = false)
        {
            var output = GetOutput(context);
            if (output != null)
            {
                ActivateOutputCore(context, output, disableDepth);
            }
        }

        /// <summary>
        /// Activates the output to the current <see cref="GraphicsDevice" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="output">The output.</param>
        /// <param name="disableDepth">if set to <c>true</c> [disable depth].</param>
        protected virtual void ActivateOutputCore(RenderContext context, RenderFrame output, bool disableDepth)
        {
            // Setup the render target
            context.GraphicsDevice.SetDepthAndRenderTargets(disableDepth ? null : output.DepthStencil, output.RenderTargets);
        }

        protected override void DrawCore(RenderContext context)
        {
            var output = GetOutput(context);
            if (output != null)
            {
                try
                {
                    context.PushParameters(Parameters);

                    ActivateOutput(context);

                    DrawCore(context, output);
                }
                finally
                {
                    context.PopParameters();

                    if (ResetGraphicsStates)
                    {
                        // Make sure that states are clean after this rendering
                        context.GraphicsDevice.ResetStates();
                    }
                }
            }
        }

        protected abstract void DrawCore(RenderContext context, RenderFrame output);

        protected override void Destroy()
        {
            if (Output != null)
            {
                Output.Dispose();
                Output = null;
            }

            base.Destroy();
        }
    }
}