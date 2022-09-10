using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.Minio;
public class PublicPolicy : IPolicy
{
    public string BucketName { get; set; }

    public string GetJsonPolicy()  => 
        string.IsNullOrWhiteSpace(this.BucketName)
        ? throw new InvalidOperationException("BucketName is null") 
        :  $@"{{
                ""Version"":""2012-10-17"",
                ""Statement"":[
                    {{
                        ""Action"":[""s3:GetBucketLocation""],
                        ""Effect"":""Allow"",
                        ""Principal"":{{""AWS"":[""*""]}},
                        ""Resource"":[
                            ""arn:aws:s3:::{this.BucketName}""
                        ],
                        ""Sid"":""""
                    }},
                    {{
                        ""Action"":[""s3:ListBucket""],
                        ""Condition"": {{
                            ""StringEquals"":{{
                                ""s3:prefix"":[""foo"",""prefix/""]
                            }}
                        }},
                        ""Effect"":""Allow"",
                        ""Principal"":{{""AWS"":[""*""]}},
                        ""Resource"":[
                            ""arn:aws:s3:::{this.BucketName}""
                        ],
                        ""Sid"":""""
                    }},
                    {{
                        ""Action"":[""s3:GetObject""],
                        ""Effect"":""Allow"",
                        ""Principal"":{{""AWS"":[""*""]}},
                        ""Resource"":[
                            ""arn:aws:s3:::{this.BucketName}/*""
                        ],
                        ""Sid"":""""
                    }}
                ]
            }}";
}
