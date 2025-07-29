namespace Seven.Boundless.Modding;

using System;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Seven.Boundless.Utility;

public class FilePathConverter : IYamlTypeConverter {
	private static readonly Type FilePathType = typeof(FilePath);
	private static readonly Type DirectoryPathType = typeof(DirectoryPath);

	public bool Accepts(Type type) {
		return type == FilePathType || type == DirectoryPathType;
	}

	public object? ReadYaml(IParser parser, Type type, ObjectDeserializer nestedObjectDeserializer) {
		if (type == FilePathType) {
			Scalar? scalar = (Scalar?)parser.Current;
			if (scalar is null) return null;

			parser.MoveNext();
			return new FilePath(scalar.Value);
		}
		else if (type == DirectoryPathType) {
			Scalar? scalar = (Scalar?)parser.Current;
			if (scalar is null) return null;

			parser.MoveNext();
			return new DirectoryPath(scalar.Value);
		}
		return null;
	}

	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedObjectSerializer) {
		if (value is null) {
			emitter.Emit(new Scalar("null"));
			return;
		}

		if (type == FilePathType) {
			FilePath path = (FilePath)value;
			if (path.Path is string pathString) {
				emitter.Emit(new Scalar(null, null, pathString, ScalarStyle.DoubleQuoted, false, true));
			}
			else {
				emitter.Emit(new Scalar("null"));
			}
		}
		else if (type == DirectoryPathType) {
			DirectoryPath path = (DirectoryPath)value;
			if (path.Path is string pathString) {
				emitter.Emit(new Scalar(null, null, pathString, ScalarStyle.DoubleQuoted, false, true));
			}
			else {
				emitter.Emit(new Scalar("null"));
			}
		}
	}
}