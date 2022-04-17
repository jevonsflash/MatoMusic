using System;
using Abp.Dependency;
using CommunityToolkit.Maui.Views;
using MatoMusic.Core.Models;
using Microsoft.Maui.Controls;


namespace MatoMusic
{
	public partial class PlaylistChoosePage : Popup, ITransientDependency
	{
		public PlaylistChoosePage()
		{
			InitializeComponent();
		}

		public event EventHandler<PlaylistInfo> OnFinished;

	}
}
