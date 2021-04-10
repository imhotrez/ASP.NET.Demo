namespace Demo.Models.Dto {
    public class Response<T> {
        public Response(T data, WebError[] errors = null) {
            Data = data;
            Errors = errors;
        }

        public T Data { get; }
        public WebError[] Errors { get; }
    }
}