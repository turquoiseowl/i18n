using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace i18n {
    public class AsyncPostbackParser {

        private string Data { get; set; }

        private List<Section> Delta { get; set; }

        public AsyncPostbackParser(string data) {
            Data = data;
            ParseDelta();
        }

        private static string FindText(string text, int location) {
            var startIndex = Math.Max(0, location - 20);
            var endIndex = Math.Min(text.Length, location + 20);
            return text.Substring(startIndex, endIndex);
        }

        private void ParseDelta() {
            int delimiterIndex, len;
            int replyIndex = 0;
            string parserErrorDetails, type, id, content;
            Delta = new List<Section>();
            while (replyIndex < Data.Length) {
                delimiterIndex = Data.IndexOf('|', replyIndex);
                if (delimiterIndex == -1) {
                    parserErrorDetails = FindText(Data, replyIndex);
                    break;
                }
                len = int.Parse(Data.Substring(replyIndex, delimiterIndex - replyIndex));
                if ((len % 1) != 0) {
                    parserErrorDetails = FindText(Data, replyIndex);
                    break;
                }
                replyIndex = delimiterIndex + 1;
                delimiterIndex = Data.IndexOf('|', replyIndex);
                if (delimiterIndex == -1) {
                    parserErrorDetails = FindText(Data, replyIndex);
                    break;
                }
                type = Data.Substring(replyIndex, delimiterIndex - replyIndex);
                replyIndex = delimiterIndex + 1;
                delimiterIndex = Data.IndexOf('|', replyIndex);
                if (delimiterIndex == -1) {
                    parserErrorDetails = FindText(Data, replyIndex);
                    break;
                }
                id = Data.Substring(replyIndex, delimiterIndex - replyIndex);
                replyIndex = delimiterIndex + 1;
                if ((replyIndex + len) >= Data.Length) {
                    parserErrorDetails = FindText(Data, Data.Length);
                    break;
                }
                content = Data.Substring(replyIndex, len);
                replyIndex += len;
                if (Data[replyIndex] != '|') {
                    parserErrorDetails = FindText(Data, replyIndex);
                    break;
                }
                replyIndex++;
                Delta.Add(new Section() {
                    Type = type,
                    Id = id,
                    Content = content
                });
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            foreach (var section in Delta) {
                sb = sb.Append(section.Content.Length)
                    .Append('|')
                    .Append(section.Type)
                    .Append('|')
                    .Append(section.Id)
                    .Append('|')
                    .Append(section.Content)
                    .Append('|');
            }
            return sb.ToString();
        }

        public class Section {
            public string Type { get; set; }
            public string Id { get; set; }
            public string Content { get; set; }
        }

        public List<Section> GetSections(string sectionType, string sectionId = null) {
            return Delta.Where(d => d.Type == sectionType && (sectionId == null || d.Id == sectionId)).ToList();
        }

        public void SetSection(string sectionName, string sectionId, string content) {
            var section = Delta.FirstOrDefault(d => d.Type == sectionName);
            section.Content = content;
        }
    }
}
