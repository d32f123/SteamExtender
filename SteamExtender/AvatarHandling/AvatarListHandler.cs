using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteamExtender.AvatarHandling
{
    /// <summary>
    /// Хранит в себе экземпляры класса Avatar
    /// Для более легкого манипулирования данными объектами
    /// </summary>
    public class AvatarListHandler
    {
        private List<Avatar> Avatars;
        private Avatar currUserAvatar;

        private List<Grid> grids;
        private Grid currUserGrid;

        private List<Border> gridBorders;
        private Border currUserBorder;



        public AvatarListHandler(IEnumerable<Grid> grids, Grid currUserGrid)
        {
            this.grids = new List<Grid>(grids);
            this.currUserGrid = currUserGrid;

            foreach(Grid grid in this.grids)
            {
                //TODO: инициализация
                //Border 
            }
        }
    }
}
