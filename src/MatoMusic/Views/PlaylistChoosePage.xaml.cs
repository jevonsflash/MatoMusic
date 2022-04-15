using System;
using Abp.Dependency;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
	public partial class PlaylistChoosePage : ContentPage, ITransientDependency
	{
		public PlaylistChoosePage()
		{
			InitializeComponent();
		}

		public event EventHandler<PlaylistInfo> OnFinished;

	}
}
