<script>
	import { goto } from '$app/navigation';
	import { onMount } from 'svelte';

	onMount(() => {
		goto('/admin');
	});
</script>

<div class="flex justify-center items-center min-h-screen">
	<h1 class="text-2xl font-semibold text-gray-700">Завантаження...</h1>
</div>
