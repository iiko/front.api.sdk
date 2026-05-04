using System.Collections.ObjectModel;
using System.Linq;
using Resto.Front.Api.Data.Brd;

namespace Resto.Front.Api.SamplePlugin.Restaurant
{
    public sealed partial class ReservesView
    {
        private readonly ObservableCollection<IReserve> reserves = new ObservableCollection<IReserve>();

        public ObservableCollection<IReserve> Reserves
        {
            get { return reserves; }
        }

        public ReservesView()
        {
            InitializeComponent();
            ReloadReserves();
        }

        internal void ReloadReserves()
        {
            reserves.Clear();
            PluginContext.Operations.GetReserves().ForEach(reserves.Add);
        }
    }
}
