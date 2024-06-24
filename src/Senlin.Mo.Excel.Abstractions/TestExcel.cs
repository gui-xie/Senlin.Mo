namespace Senlin.Mo.Excel.Abstractions;

public class TestExcel
{
    public void A()
    {
        const string json = """
                            {
                              "name": "user",
                              "uniqueKey": [
                                "no"
                              ],
                              "columns": [
                                {
                                  "name": "no",
                                  "displayName": "学生编号",
                                  "width": 50,
                                  "type": "string",
                                  "constraints": {
                                    "required": true,
                                    "minLength": 10,
                                    "maxLength": 10
                                  }
                                },
                                {
                                  "name": "name",
                                  "displayName": "姓名",
                                  "width": 100,
                                  "type": "string",
                                  "comment": "用户姓名",
                                  "constraints": {
                                    "required": true,
                                    "minLength": 2,
                                    "maxLength": 50
                                  }
                                },
                                {
                                  "name": "age",
                                  "displayName": "年龄",
                                  "type": "number",
                                  "constraints": {
                                    "minValue": 18,
                                    "maxValue": 100
                                  }
                                },
                                {
                                  "name": "gender",
                                  "displayName": "性别",
                                  "type": "enum"
                                },
                                {
                                  "name": "birthday",
                                  "displayName": "生日",
                                  "type": "date"
                                },
                                {
                                  "name": "avatar",
                                  "displayName": "头像",
                                  "type": "image"
                                }
                              ]
                            }
                            """;
        
    }

    private class ExcelJson
    {
        public string Name { get; set; } = string.Empty;

        public string[] UniqueKey { get; set; } = [];

        public ExcelColumnJson[] Columns { get; set; } = [];
    }

    private class ExcelColumnJson
    {
        public string Name { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int Width { get; set; }

        public string Type { get; set; } = string.Empty;

        public Dictionary<string, object> Validations { get; set; } = new();
    }
}