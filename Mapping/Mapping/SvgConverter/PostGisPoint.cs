namespace Mapping.SvgConverter
{
    /// <summary>
    /// Storage class for simple point data.
    /// Created by Timothy J Cowen.
    /// </summary>
    public class PostGisPoint
    {
        private double x;
        private double y;

        public double X
        {
            get => x;
            set
            {
                x = value;
                IsXSet = true;
            }
        }

        public double Y
        {
            get => y;
            set
            {
                y = value;
                IsYSet = true;
            }
        }

        public double Longitude => X;
        public double Latitude => Y;

        public bool IsXSet { get; private set; }
        public bool IsYSet { get; private set; }
    }
}
