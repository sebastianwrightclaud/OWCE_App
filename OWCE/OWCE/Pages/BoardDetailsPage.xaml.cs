using System;
using System.Collections.Generic;


namespace OWCE.Pages
{
    public partial class BoardDetailsPage : ContentPage
    {
        public BoardDetailsPage(OWBoard board)
        {
            BindingContext = board;

            InitializeComponent();

            ToolbarItems.Add(new ToolbarItem("Cancel", null, () =>
            {
                Navigation.PopModalAsync();
            }));
        }
    }
}
