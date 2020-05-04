using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProtoSmasher_Lib.Events
{
    public class Settings
    {
        [JsonProperty("explorer_open")]
        public bool ExplorerOpen { get; set; }

        [JsonProperty("watermark_position")]
        public long WatermarkPosition { get; set; }

        [JsonProperty("pin_output")]
        public bool PinOutput { get; set; }

        [JsonProperty("autoscroll_output")]
        public bool AutoscrollOutput { get; set; }

        [JsonProperty("autoscroll_console")]
        public bool AutoscrollConsole { get; set; }

        [JsonProperty("autoscroll_chat")]
        public bool AutoscrollChat { get; set; }

        [JsonProperty("watermark_enabled")]
        public bool WatermarkEnabled { get; set; }

        [JsonProperty("output_redirection")]
        public bool OutputRedirection { get; set; }

        [JsonProperty("esp_enable")]
        public bool EspEnable { get; set; }

        [JsonProperty("esp_base_color_on_team_color")]
        public bool EspBaseColorOnTeamColor { get; set; }

        [JsonProperty("esp_tracer_lines")]
        public bool EspTracerLines { get; set; }

        [JsonProperty("esp_mode_2d")]
        public bool EspMode2D { get; set; }

        [JsonProperty("esp_hide_dead_players")]
        public bool EspHideDeadPlayers { get; set; }

        [JsonProperty("esp_text_outline")]
        public bool EspTextOutline { get; set; }

        [JsonProperty("esp_hide_same_team")]
        public bool EspHideSameTeam { get; set; }

        [JsonProperty("esp_text_size")]
        public long EspTextSize { get; set; }

        [JsonProperty("esp_rainbow_color")]
        public bool EspRainbowColor { get; set; }

        [JsonProperty("esp_info_visible")]
        public bool EspInfoVisible { get; set; }

        [JsonProperty("chams_enable")]
        public bool ChamsEnable { get; set; }

        [JsonProperty("chams_rainbow")]
        public bool ChamsRainbow { get; set; }

        [JsonProperty("chams_base_color_on_team_color")]
        public bool ChamsBaseColorOnTeamColor { get; set; }

        [JsonProperty("chams_always_on_top")]
        public bool ChamsAlwaysOnTop { get; set; }

        [JsonProperty("chams_hide_same_team")]
        public bool ChamsHideSameTeam { get; set; }

        [JsonProperty("chams_transparency")]
        public long ChamsTransparency { get; set; }

        [JsonProperty("chams_color")]
        public long ChamsColor { get; set; }

        [JsonProperty("crosshair")]
        public bool Crosshair { get; set; }

        [JsonProperty("crosshair_thickness")]
        public long CrosshairThickness { get; set; }

        [JsonProperty("crosshair_length")]
        public long CrosshairLength { get; set; }

        [JsonProperty("crosshair_offset")]
        public long CrosshairOffset { get; set; }

        [JsonProperty("crosshair_dot")]
        public bool CrosshairDot { get; set; }

        [JsonProperty("crosshair_color")]
        public long CrosshairColor { get; set; }

        [JsonProperty("aimbot_enable")]
        public bool AimbotEnable { get; set; }

        [JsonProperty("aimbot_ignore_team")]
        public bool AimbotIgnoreTeam { get; set; }

        [JsonProperty("aimbot_visible_only")]
        public bool AimbotVisibleOnly { get; set; }

        [JsonProperty("aimbot_raycast_check")]
        public bool AimbotRaycastCheck { get; set; }

        [JsonProperty("aimbot_smoothing")]
        public long AimbotSmoothing { get; set; }

        [JsonProperty("aimbot_distance")]
        public long AimbotDistance { get; set; }

        [JsonProperty("aimbot_mode")]
        public long AimbotMode { get; set; }

        [JsonProperty("aimbot_look_mode")]
        public long AimbotLookMode { get; set; }

        [JsonProperty("triggerbot_enable")]
        public bool TriggerbotEnable { get; set; }

        [JsonProperty("triggerbot_ignore_team")]
        public bool TriggerbotIgnoreTeam { get; set; }

        [JsonProperty("triggerbot_max_dist")]
        public long TriggerbotMaxDist { get; set; }

        [JsonProperty("triggerbot_part")]
        public long TriggerbotPart { get; set; }

        [JsonProperty("fps_unlock")]
        public bool FpsUnlock { get; set; }

        [JsonProperty("fps_value")]
        public long FpsValue { get; set; }

        [JsonProperty("asus_walls_enabled")]
        public bool AsusWallsEnabled { get; set; }

        [JsonProperty("asus_walls_own_char_invis")]
        public bool AsusWallsOwnCharInvis { get; set; }

        [JsonProperty("asus_walls_percent")]
        public long AsusWallsPercent { get; set; }

        [JsonProperty("disable_easter_egg")]
        public bool DisableEasterEgg { get; set; }

        [JsonProperty("remotespy_enable")]
        public bool RemotespyEnable { get; set; }

        [JsonProperty("ui_passthrough")]
        public bool UiPassthrough { get; set; }

        [JsonProperty("radar_enabled")]
        public bool RadarEnabled { get; set; }

        [JsonProperty("radar_zoom")]
        public long RadarZoom { get; set; }

        [JsonProperty("radar_show_name")]
        public bool RadarShowName { get; set; }

        [JsonProperty("radar_base_color_on_team")]
        public bool RadarBaseColorOnTeam { get; set; }

        [JsonProperty("radar_color")]
        public long RadarColor { get; set; }


        private readonly ProtoSmasherLib _protoSmasherLib;

        private static readonly JsonPropertyCollection _jsonProperties =
            ((JsonObjectContract)(new DefaultContractResolver()).ResolveContract(typeof(Settings)))
            .Properties;

        private string GetJsonPropertyName(string name)
        {
            var property = _jsonProperties.FirstOrDefault(x => x.UnderlyingName == name);
            if (property == null || property.Equals(default))
                throw new Exception("Invalid property name");

            return property.PropertyName;
        }

        private object GetPropertyValue(string property)
        {
            var value = typeof(Settings).GetProperty(property)?.GetValue(this, null);

            // ProtoSmasher interprets bools as ints
            if (value is bool)
                return (bool)value ? 1 : 0;
            
            return value;
        }

        /// <summary>
        /// Gets all the real settings names as a dictionary from ClassPropertyName => RealPropertyName
        /// </summary>
        /// <returns>Map of setting names</returns>
        public Dictionary<string, string> GetSettingsMap()
        {
            return _jsonProperties.ToDictionary(p => p.UnderlyingName, p => p.PropertyName);
        }

        /// <summary>
        /// Updates the changed property in ProtoSmasher
        /// </summary>
        /// <param name="property">Name of Property that changed</param>
        public void RaiseProperty(string property)
        {
            _protoSmasherLib.UpdateSetting(GetJsonPropertyName(property),
                GetPropertyValue(property));
        }

        /// <summary>
        /// See RaiseProperty, but async
        /// </summary>
        /// <param name="property">Name of Property that changed</param>
        /// <returns>Awaitable Task</returns>
        public Task RaisePropertyAsync(string property)
        {
            return _protoSmasherLib.UpdateSettingAsync(GetJsonPropertyName(property),
                GetPropertyValue(property));
        }

        internal Settings(ProtoSmasherLib protoSmasherLib, string json)
        {
            // This is required to make RaiseProperty to work
            _protoSmasherLib = protoSmasherLib;
            JsonConvert.PopulateObject(json, this, SettingsConverter.Settings);
        }

        public string ToJson() 
        {
            return JsonConvert.SerializeObject(this, SettingsConverter.Settings);
        }
    }

    internal static class SettingsConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
