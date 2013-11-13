
namespace AudioClassification {
    public class Sound {
        public string filename;
        public string fullname;

        public Sound(string filename, string fullname) {
            this.filename = filename;
            this.fullname = fullname;
        }

        public override string ToString() {
            return filename;
        }
    }
}
