"""Shared PNG ancillary-chunk handling for release tooling."""
import struct

PROFILE_INFO = {
    'dpi',
    'srgb',
    'gamma',
    'icc_profile',
    'chromaticity',
    'exif',
    'xmp',
    'XML:com.adobe.xmp',
}


def ancillary_chunks(data, selected):
    if data[:8] != b'\x89PNG\r\n\x1a\n':
        return []
    chunks, offset = [], 8
    while offset + 12 <= len(data):
        size = struct.unpack_from('>I', data, offset)[0]
        end = offset + size + 12
        kind = data[offset + 4:offset + 8].decode('ascii', 'replace')
        if end > len(data):
            raise ValueError('Invalid PNG chunk length')
        if kind in selected:
            chunks.append(data[offset:end])
        offset = end
        if kind == 'IEND':
            break
    return chunks


def insert_before_idat(data, chunks):
    if not chunks:
        return data
    result, offset, inserted = bytearray(data[:8]), 8, False
    while offset + 12 <= len(data):
        size = struct.unpack_from('>I', data, offset)[0]
        end = offset + size + 12
        kind = data[offset + 4:offset + 8]
        if kind == b'IDAT' and not inserted:
            result.extend(b''.join(chunks))
            inserted = True
        result.extend(data[offset:end])
        offset = end
        if kind == b'IEND':
            break
    if not inserted:
        raise ValueError('PNG output has no IDAT chunk')
    return bytes(result)
