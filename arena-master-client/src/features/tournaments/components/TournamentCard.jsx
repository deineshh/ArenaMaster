import { Card, Text, Badge, Image, Group, Box } from '@mantine/core';
import { Link } from 'react-router';
import { IconUsers, IconCalendar, IconDeviceGamepad, IconTrophy } from '@tabler/icons-react';
import { uploadUrl } from '../../../utils/bracketHelpers';
import { formatDate } from '../../../utils/formatDate';
import { statusColors, statusLabels } from '../../../styles/component-tokens';

export function TournamentCard({ tournament: t }) {
  const maxParts = t.maxParticipants || 16;
  const progressPercent = Math.min(((t.participantsCount || 0) / maxParts) * 100, 100);

  // Determinate gradient and glow based on status
  const getStatusGlow = (status) => {
    if (status === 'ongoing') return '0 0 15px rgba(244, 63, 94, 0.4)';
    if (status === 'registration') return '0 0 15px rgba(124, 58, 237, 0.4)';
    return 'none';
  };

  const getFormatLabel = (fmt) => {
    if (fmt === 'single_elimination') return 'Single Elimination';
    if (fmt === 'double_elimination') return 'Double Elimination';
    return fmt;
  };

  return (
    <Card
      component={Link}
      to={`/tournaments/${t.slug}`}
      withBorder
      padding={0} /* Full-bleed layout */
      className="hover-glow-neon fade-in"
      style={{
        textDecoration: 'none',
        background: 'var(--surface-elevated)',
        borderColor: 'var(--color-border-subtle)',
        borderRadius: '12px',
        overflow: 'hidden',
        transition: 'transform var(--transition-gaming), border-color var(--transition-fast), box-shadow var(--transition-fast)',
        boxShadow: '0 4px 20px rgba(0, 0, 0, 0.4)'
      }}
    >
      {/* 1. Cover Image Section with diagonal esports grids */}
      <Box style={{ position: 'relative', overflow: 'hidden', height: '135px' }}>
        {t.coverUrl ? (
          <Image
            src={uploadUrl(t.coverUrl)}
            h={135}
            w="100%"
            alt={t.title}
            style={{
              objectFit: 'cover',
              transition: 'transform 0.4s ease',
              filter: t.status === 'finished' ? 'grayscale(0.6)' : 'none'
            }}
            className="hover-scale"
          />
        ) : (
          <Box style={{
            height: '100%',
            background: 'linear-gradient(135deg, var(--surface-tertiary) 0%, var(--surface-secondary) 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
          }}>
            <IconTrophy size={40} color="var(--color-primary)" opacity={0.3} />
          </Box>
        )}

        {/* Diagonal Scanline Overlay */}
        <Box style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'linear-gradient(rgba(15, 15, 35, 0.2) 0%, rgba(15, 15, 35, 0.7) 100%)',
          pointerEvents: 'none'
        }} />

        {/* Glowing Status Badge placed on the top left */}
        <Badge
          style={{
            position: 'absolute',
            top: '12px',
            left: '12px',
            textShadow: '0 1px 2px rgba(0,0,0,0.5)',
            boxShadow: getStatusGlow(t.status),
            fontFamily: 'Chakra Petch, sans-serif',
            fontWeight: 700
          }}
          color={statusColors[t.status] || 'gray'}
          variant="filled"
          size="sm"
        >
          {statusLabels[t.status] || t.status}
        </Badge>
      </Box>

      {/* 2. Content Details Section */}
      <Box p="md" style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        {/* Title */}
        <Text
          style={{
            fontFamily: 'Chakra Petch, sans-serif',
            fontSize: '16px',
            fontWeight: 700,
            color: 'var(--color-text)',
            letterSpacing: '0.5px'
          }}
          lineClamp={1}
        >
          {t.title}
        </Text>

        {/* Discipline and Format Subtitles */}
        <Group gap="xs" wrap="nowrap">
          <Group gap={4} wrap="nowrap" style={{ flexShrink: 0 }}>
            <IconDeviceGamepad size={14} color="var(--color-accent)" />
            <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 600, color: 'var(--color-text-muted)' }}>
              {t.disciplineName}
            </Text>
          </Group>
          <Text size="xs" style={{ color: 'var(--color-border)' }}>·</Text>
          <Text size="xs" style={{
            fontFamily: 'Chakra Petch, sans-serif',
            fontWeight: 500,
            color: 'var(--color-text-muted)',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          }}>
            {getFormatLabel(t.format)}
          </Text>
        </Group>

        {/* Scheduled Start Date */}
        <Group gap={4} wrap="nowrap">
          <IconCalendar size={14} color="var(--color-primary)" />
          <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', color: 'var(--color-text-muted)' }}>
            {formatDate(t.startsAt)}
          </Text>
        </Group>

        {/* 3. Linear neon progress registration bar */}
        <Box style={{ marginTop: '4px' }}>
          <Group justify="space-between" mb={4}>
            <Group gap={4} align="center">
              <IconUsers size={13} color="var(--color-text-muted)" />
              <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 600, color: 'var(--color-text-muted)' }}>
                Реєстрація
              </Text>
            </Group>
            <Text size="xs" style={{ fontFamily: 'Chakra Petch, sans-serif', fontWeight: 700,             color: 'var(--color-text)' }}>
              {t.participantsCount || 0} / {maxParts}
            </Text>
          </Group>
          {/* Visual progress bar bar */}
          <Box style={{
            height: '6px',
            background: 'var(--surface-hover)',
            borderRadius: '3px',
            overflow: 'hidden'
          }}>
            <Box style={{
              width: `${progressPercent}%`,
              height: '100%',
              background: 'linear-gradient(90deg, #7C3AED 0%, #F43F5E 100%)',
              borderRadius: '3px',
              transition: 'width 0.4s cubic-bezier(0.34, 1.56, 0.64, 1)'
            }} />
          </Box>
        </Box>
      </Box>
    </Card>
  );
}
