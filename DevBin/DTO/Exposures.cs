namespace DevBin.DTO
{
    public class Exposures
    {
        /// <summary>
        /// Id of the exposure
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Pretty name of the exposure
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this exposure allows the paste to be publicly available
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Whether this exposure allows the paste to be created only by registered users
        /// </summary>
        public bool RegisteredOnly { get; set; }

        /// <summary>
        /// Whether this exposure allows the paste to be modified by the owner
        /// </summary>
        public bool AllowEdit { get; set; }

        /// <summary>
        /// Whether this exposure allows the paste to be seen only to the owner
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
