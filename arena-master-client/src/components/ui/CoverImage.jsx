import { Image } from '@mantine/core';
import { uploadUrl } from '../../utils/bracketHelpers';

export function CoverImage({ src, height = 200, alt = '' }) {
  return (
    <Image
      src={uploadUrl(src)}
      h={height}
      radius="md"
      mb="lg"
      alt={alt}
      className="transition-all"
      style={{
        border: '1px solid var(--color-border)',
        objectFit: 'cover',
      }}
    />
  );
}
