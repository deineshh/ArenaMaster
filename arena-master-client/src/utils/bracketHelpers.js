export function groupMatchesByRound(matches) {
  const rounds = {};
  for (const m of matches ?? []) {
    const key = `${m.bracketSide}-${m.round}`;
    if (!rounds[key]) rounds[key] = [];
    rounds[key].push(m);
  }
  return rounds;
}

export const apiBase = import.meta.env.VITE_API_URL || '';

export function uploadUrl(path) {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  return `${apiBase}${path}`;
}
