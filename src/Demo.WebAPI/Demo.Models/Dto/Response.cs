namespace Demo.Models.Dto {
    public class Response<T> {
        public Response(T data, Error error = null) {
            Data = data;
            Error = error;
        }

        public T Data { get; }
        public Error Error { get; }
    }
}