import { Container, Paper, Title, Textarea, TextInput, Button, Stack, Avatar, Group, FileInput } from '@mantine/core';
import { useForm } from 'react-hook-form';
import { useState } from 'react';
import { useGetMeQuery } from '../../api/authApi';
import { useUpdateProfileMutation, useUploadAvatarMutation } from '../../api/usersApi';
import { ProtectedRoute } from '../../components/ui/ProtectedRoute';
import { notifications } from '@mantine/notifications';
import { useNavigate } from 'react-router';
import { uploadUrl } from '../../utils/bracketHelpers';

export default function EditProfilePage() {
  const { data: user } = useGetMeQuery();
  const [update] = useUpdateProfileMutation();
  const [uploadAvatar, { isLoading: uploading }] = useUploadAvatarMutation();
  const navigate = useNavigate();
  const [avatarFile, setAvatarFile] = useState(null);
  const { register, handleSubmit } = useForm({
    values: { bio: user?.bio ?? '', discordUrl: user?.discordUrl ?? '' },
  });

  const handleAvatarUpload = async () => {
    if (!avatarFile) return;
    try {
      await uploadAvatar(avatarFile).unwrap();
      notifications.show({ message: 'Аватар оновлено', color: 'green' });
      setAvatarFile(null);
    } catch {
      notifications.show({ message: 'Помилка завантаження', color: 'red' });
    }
  };

  const onSubmit = async (values) => {
    try {
      await update(values).unwrap();
      notifications.show({ message: 'Профіль оновлено', color: 'green' });
      navigate(`/profile/${user.username}`);
    } catch {
      notifications.show({ message: 'Помилка', color: 'red' });
    }
  };

  const inputStyles = {
    input: {
      background: 'var(--color-bg)',
      borderColor: 'var(--color-border)',
      color: 'var(--color-text)',
      transition: 'all var(--transition-fast)',
    },
    inputFocus: { borderColor: 'var(--color-primary)', boxShadow: 'var(--glow-primary)' },
    label: { color: 'var(--color-text-muted)', marginBottom: 4 },
  };

  return (
    <ProtectedRoute>
      <Container size="sm" py="xl" className="fade-in">
        <Paper
          p="xl"
          withBorder
          style={{
            background: 'var(--color-bg-card)',
            borderColor: 'var(--color-border)',
            borderLeft: '3px solid var(--color-primary)',
          }}
        >
          <Title order={2} mb="lg" className="gradient-text">
            Редагувати профіль
          </Title>

          <Stack mb="lg">
            <Title order={4} style={{ color: 'var(--color-text-muted)' }}>Аватар</Title>
            <Group>
              <Avatar
                src={avatarFile ? URL.createObjectURL(avatarFile) : uploadUrl(user?.avatarUrl)}
                alt={user?.username}
                size={120}
                style={{ border: '2px solid var(--color-primary)' }}
              />
              <Stack gap="xs">
                <FileInput
                  accept="image/png,image/jpeg,image/webp"
                  placeholder="Вибрати файл"
                  value={avatarFile}
                  onChange={setAvatarFile}
                  styles={{
                    input: {
                      background: 'var(--color-bg)',
                      borderColor: 'var(--color-border)',
                      color: 'var(--color-text)',
                    },
                  }}
                />
                <Button
                  size="sm"
                  onClick={handleAvatarUpload}
                  loading={uploading}
                  disabled={!avatarFile}
                  style={{
                    background: 'var(--color-primary)',
                    transition: 'all var(--transition-normal)',
                  }}
                >
                  Завантажити
                </Button>
              </Stack>
            </Group>
          </Stack>

          <form onSubmit={handleSubmit(onSubmit)}>
            <Stack>
              <Textarea label="Bio" styles={inputStyles} {...register('bio')} />
              <TextInput label="Discord URL" styles={inputStyles} {...register('discordUrl')} />
              <Button
                type="submit"
                style={{
                  background: 'var(--brand-gradient)',
                  transition: 'all var(--transition-normal)',
                }}
              >
                Зберегти
              </Button>
            </Stack>
          </form>
        </Paper>
      </Container>
    </ProtectedRoute>
  );
}
