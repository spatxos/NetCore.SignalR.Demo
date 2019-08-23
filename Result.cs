namespace Entity
{
    [Serializable]
    public class BaseEntity
    {
        public override string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch
            {
                return "";
            }
        }
    }
    
    public class Response: BaseEntity
    {
        public Requestcode code { get; set; }
        public string msg { get; set; }

    }

        public enum Requestcode
        {
            [Description("request unauthorized")]
            未授权的请求 = -3,//未授权的请求

            [Description("request error")]
            请求错误 = -2,//未授权的请求

            [Description("system busy")]
            系统繁忙 = -1,//系统繁忙，此时请开发者稍候再试

            [Description("success")]
            请求成功 = 0,

            [Description("request unauthorized parameter error")]
            请求授权参数错误 = 1,

            [Description("whitelist IP address is not")]
            调用接口的IP地址不在白名单中 = 2//调用接口的IP地址不在白名单中，请在接口IP白名单中进行设置。


        }


    public class Result : Response
    {

        private Requestcode Code;
        public Requestcode code
        {
            get { return Code; }
            set
            {
                Code = value;
                msg = EnumHelper<Requestcode>.GetEnumDescription(Code);
            }
        }

        private string Msg;
        public string msg
        {
            get { return Msg; }
            set
            {
                if (!string.IsNullOrWhiteSpace(Msg))
                {
                    Msg = value;
                }
                else
                {
                    Msg = EnumHelper<Requestcode>.GetEnumDescription(code);
                }
            }
        }

        private object data;
        public object Data
        {
            get { return data; }
            set
            {
                if (value != null)
                {
                    try
                    {
                        data = value;
                    }
                    catch
                    {
                        data = value;
                    }
                }
            }
        }

        public Result()
        {
            this.code = Requestcode.请求成功;
            this.msg = EnumHelper<Requestcode>.GetEnumDescription(this.code);
        }
        
        public Result(Requestcode errcode)
        {
            this.code = errcode;
            this.msg = EnumHelper<Requestcode>.GetEnumDescription(errcode);
            this.Data = null;
        }

        public Result(Requestcode errcode, string errmsg)
        {
            this.code = errcode;
            this.msg = errmsg;
            this.Data = null;
        }


        public Result(Requestcode errcode, object data)
        {
            this.code = errcode;
            this.msg = EnumHelper<Requestcode>.GetEnumDescription(errcode);
            this.Data = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
        }

        public Result(Requestcode errcode, string errmsg, object data)
        {
            this.code = errcode;
            this.msg = errmsg;
            this.Data = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
        }

        public Result(object data)
        {
            this.code = Requestcode.请求成功;
            this.msg = EnumHelper<Requestcode>.GetEnumDescription(Requestcode.请求成功);
            this.Data = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
        }

        public Result SetDate(object data)
        {
            this.code = Requestcode.请求成功;
            this.msg = EnumHelper<Requestcode>.GetEnumDescription(Requestcode.请求成功);
            this.Data = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
            return this;
        }
    }

}
