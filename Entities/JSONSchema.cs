using PasswordManager.DTOs;
using System.Collections.Generic;

namespace PasswordManager.Entities
{
    public class JSONSchema
    {
        public SettingsDTO Settings { get; set; }
        public ParamsDTO Params { get; set; }
        public List<TableRowDTO> Data { get; set; }

        public JSONSchema()
        {
            Params = null;
            Data = null;
        }
    }
}
