import { format, formatDistanceToNow, parseISO } from 'date-fns';

/**
 * Format a date string or Date object to a readable format
 * @param date - ISO date string or Date object
 * @param formatString - Optional format string (defaults to 'PPP' - e.g., "April 29, 2024")
 * @returns Formatted date string
 */
export function formatDate(date: string | Date, formatString: string = 'PPP'): string {
	try {
		const dateObj = typeof date === 'string' ? parseISO(date) : date;
		return format(dateObj, formatString);
	} catch (error) {
		console.error('Error formatting date:', error);
		return 'Invalid date';
	}
}

/**
 * Format a date string or Date object to a readable datetime format
 * @param date - ISO date string or Date object
 * @returns Formatted datetime string (e.g., "Apr 29, 2024, 3:45 PM")
 */
export function formatDateTime(date: string | Date): string {
	return formatDate(date, 'PPp');
}

/**
 * Format a date string or Date object as relative time
 * @param date - ISO date string or Date object
 * @returns Relative time string (e.g., "2 hours ago", "in 3 days")
 */
export function formatRelativeTime(date: string | Date): string {
	try {
		const dateObj = typeof date === 'string' ? parseISO(date) : date;
		return formatDistanceToNow(dateObj, { addSuffix: true });
	} catch (error) {
		console.error('Error formatting relative time:', error);
		return 'Invalid date';
	}
}

/**
 * Format a date string or Date object to a short format
 * @param date - ISO date string or Date object
 * @returns Short formatted date string (e.g., "04/29/2024")
 */
export function formatShortDate(date: string | Date): string {
	return formatDate(date, 'P');
}

/**
 * Format a date string or Date object to a time format
 * @param date - ISO date string or Date object
 * @returns Time string (e.g., "3:45 PM")
 */
export function formatTime(date: string | Date): string {
	return formatDate(date, 'p');
}
