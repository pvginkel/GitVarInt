# GitVarInt

GitVarInt is a variable length encoder and decoder for Git style variable length integers using Protobuf's ZigZag encoding for singed integers.

The main difference between the Git scheme and the base Protobuf scheme is that the Git scheme packs more data into fewer bytes for low values. It does so by using the length of the encoded value as information. E.g. for a variable length integer of 2 bytes, the resulting value is offset by 128 because the full range of 127 values fit within the first byte already. A side effect of this is that every value has a canonical encoding, meaning that the same value cannot be encoded in longer strings.

See the [Variable-length quantity](https://en.wikipedia.org/wiki/Variable-length_quantity) Wikipedia article for more information.

This library has encoding and decoding methods for `int`, `uint`, `long` and `ulong` values. If you know the encoded values will always be positive, ensure you use the unsigned versions as these will encode more data in fewer bytes.